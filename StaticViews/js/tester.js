"use strict";

/**
 * Created by William Le.
 * https://github.com/DoggySazHi/Modulr
 * mukyu~
 */

onInitTester();

let resetTime = new Date();
let testsRemaining = 0;
let currentTest = 0;
let websocketBuffer = [];

function onInitTester() {
    bindButtons();
    bindUploads();
    onGoogleReady.push(getAllTests);
    onGoogleReady.push(getAttemptsLeft);
    console.info("Initialized tester script!");
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", (e) => {
        e.preventDefault();
        submit();
    }, false);
}

function bindUploads() {
    document.querySelectorAll("input[type='file']").forEach((input) => {
        input.addEventListener("change", (e) => {
            if (e.target.value === "")
                e.target.parentNode.className = "input normal";
            else
                e.target.parentNode.className = "input success";
        }, false);
    });
}

function initWebsocket() {
    if(typeof connectionId === "undefined")
        return;
    connection.on("ReceiveUpdate", (data) => {
        websocketBuffer.push(data);
        displayOutput(websocketBuffer.join('\n'));
    });
}

function submit() {
    let data = new FormData();
    websocketBuffer = [];
    
    let fileInputs = document.querySelectorAll("input[type='file']");
    for (let input of fileInputs) {
        data.append("FileNames", input.name);
        if (input.files.length === 0) {
            console.error("User missed " + input.name + "!");
            triggerPopup("Mukyu~", "You need to attach a file for " + input.name + "!");
            return;
        }
        for (let i = 0; i < input.files.length; i++)
            data.append("Files", input.files[i]);
        data.append("IsTester", JSON.stringify(false));
    }
    data.append("TestID", JSON.stringify(currentTest));
    data.append("AuthToken", getLoginToken());
    if(typeof connectionId !== "undefined")
        data.append("ConnectionID", connectionId);

    document.getElementById("result").innerHTML = "Now loading...";
    document.getElementById("submit").disabled = true;

    fetch("/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR " + response.status);
        return response.text();
    })
    .then((formatted) => {
        displayOutput(formatted)
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 400:
                    error = "The server didn't like your files... did you miss something?";
                    break;
                case 403:
                    error = "Either you are on a cooldown, or your login credentials failed.\nIf the former isn't true, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not locate the uploader... try refreshing the page?";
                    break;
                case 500:
                case 502:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    })
    .finally(() => {
        document.getElementById("submit").disabled = false;
        getAttemptsLeft();
    });
}

function displayOutput(data) {
    let text = data.toString();
    // Get first word
    let key = text.substr(0, text.indexOf(" "));
    // Split by first word
    let pattern = new RegExp("(?=" + key + ")", "g");
    let processed = text.split(pattern);
    let output = document.getElementById("result");
    
    output.innerHTML = "";

    let button;
    let content;

    for(let i = 0; i < processed.length; i++) {
        let item = processed[i];

        if (i % 2 === 0) {
            let title = "Test Results";
            let fileName = item.match(/COMPILING (\S*\.java)/);
            if (fileName != null && fileName.length >= 2)
                title = "Compiler Info for " + fileName[1];

            button = document.createElement("button");
            button.className = "collapse default";
            button.innerHTML = title;
            content = document.createElement("div");
            content.className = "collapse-content";

            if (i >= processed.length - 2)
                button.classList.toggle("active");

            output.appendChild(button);
            output.appendChild(content);
        }

        content.innerHTML += item;
        if (button.classList.contains("active"))
            content.style.maxHeight = content.scrollHeight + "px";
    }

    registerCollapsibles();
}

function getTest(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    fetch("/Tester/GetTest", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "AuthToken": getLoginToken(),
            "TestID": id
        })
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        if(!formatted.hasOwnProperty("requiredFiles")) {
            console.error("Could not find the required files from tester!");
            return;
        }
        generateInputs(formatted.requiredFiles);
        bindUploads();
        currentTest = id;
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not locate test, please try another one!";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    });
}

function getAllTests() {
    clearInputs();
    fetch("/Tester/GetAllTests", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "AuthToken": getLoginToken()
        })
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        generateList(formatted);
        bindUploads();
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not fetch tests because none were found!";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    });
}

function getAttemptsLeft() {
    fetch("/Users/GetTimeout", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(getLoginToken())
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        resetTime = new Date(Date.now() + formatted.milliseconds);
        testsRemaining = formatted.testsRemaining;
        updateAttemptsVisual();
        setInterval(updateTimer, 1000);
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not fetch tests because none were found!";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    });
}

function updateTimer() {
    // Oh no, timezones!
    let difference = resetTime - new Date();
    let output = document.getElementById("time");
    if (difference < 0) {
        output.innerHTML = "";
        updateAttemptsVisual();
        return;
    }
    output.innerHTML = "Reset in " + new Date(difference).toISOString().substr(11, 8);
}

function clearInputs() {
    let inputArea = document.getElementById("fileInputs");
    inputArea.innerHTML = "";
    document.getElementById("submit").disabled = true;
}

function generateInputs(names) {
    let inputArea = document.getElementById("fileInputs");
    for (let file of names) {
        let label = document.createElement("label");
        label.className = "input normal";
        let input = document.createElement("input");
        input.type = "file";
        input.name = file;
        label.innerHTML = file;
        label.appendChild(input);
        inputArea.appendChild(label);
    }
    document.getElementById("submit").disabled = false;
}

function generateList(tests) {
    let list = document.getElementById("tests-available");
    for(let test of tests) {
        let testBtn = document.createElement("button");
        testBtn.className = "default form-control";
        testBtn.innerHTML = test.name;
        testBtn.name = test.id;
        testBtn.addEventListener("click", (e) => {
            getTest(e.target.name);
        })
        list.appendChild(testBtn);
    }
}

function updateAttemptsVisual() {
    if (testsRemaining <= 0)
        document.getElementById("attempts").innerHTML = "";
    else
        document.getElementById("attempts").innerHTML = "Attempts Left: " + testsRemaining;
}