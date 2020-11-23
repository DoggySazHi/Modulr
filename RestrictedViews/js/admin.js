"use strict";

onInitAdmin();

let currentTest = 0;

function onInitAdmin() {
    bindButtons();
    bindUploads();
    onGoogleReady.push(getAllTestsAdmin);
    console.info("Initialized admin script!");
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
    document.getElementById("submit").classList.remove("hidden");
}

function clearInputs() {
    let inputArea = document.getElementById("fileInputs");
    inputArea.innerHTML = "";
    let submit = document.getElementById("submit");
    submit.disabled = true;
    submit.classList.add("hidden");
}

function generateInputs(names) {
    let inputArea = document.getElementById("fileInputs");
    for (let file of names) {
        let input = document.createElement("input");
        input.type = "file";
        
        let label = document.createElement("label");
        label.className = "input";
        
        if(file.hasOwnProperty("exists")) {
            if(!file.exists) {
                label.classList.add("danger");
                label.innerHTML += " &#10060;";
            }
            else {
                label.classList.add("success");
                label.innerHTML += " &#10004;";
            }
            input.name = file.file;
            label.innerHTML = file.file;
        }
        else {
            label.classList.add("normal");
            input.name = file;
            label.innerHTML = file;
        }

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
        if(!test.valid) {
            testBtn.classList.add("danger");
            testBtn.innerHTML += " &#10060;";
        }
        else {
            testBtn.classList.add("normal");
            testBtn.innerHTML += " &#10004;";
        }
        testBtn.addEventListener("click", (e) => {
            getTestAdmin(e.target.name);
        })
        list.appendChild(testBtn);
    }
}

function getAllTestsAdmin() {
    clearInputs();
    fetch("/Admin/Tester/GetAll", {
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

function getTestAdmin(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    fetch("/Admin/Tester/Get", {
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
        if(!formatted.hasOwnProperty("testerFiles")) {
            console.error("Could not find the tester files from tester!");
            return;
        }
        if(!formatted.hasOwnProperty("requiredFiles")) {
            console.error("Could not find the required files from tester!");
            return;
        }
        
        let inputArea = document.getElementById("fileInputs");
        let testerTitle = document.createElement("h4");
        testerTitle.innerHTML = "Testers"
        let bar = document.createElement("hr");
        let requiredTitle = document.createElement("h4");
        requiredTitle.innerHTML = "Required"
        inputArea.appendChild(testerTitle);
        generateInputs(formatted.testerFiles);
        inputArea.appendChild(bar);
        inputArea.appendChild(requiredTitle);
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

function submit() {
    
}