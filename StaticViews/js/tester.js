"use strict";

/**
 * Created by William Le.
 * https://github.com/DoggySazHi/Modulr
 * mukyu~
 */

import {getLoginToken, onGoogleReady} from "./google.js";
import {registerCollapsibles, triggerPopup, handleErrors} from "./main.js";
import {onSocketReady, connectionId} from "./websocket.js";

onInitTester();

let resetTime = new Date();
let intervalTimer = 0;
let testsRemaining = 0;
let currentTest = 0;
let websocketBuffer = [];

let lastAnimatedBox = -1;

function onInitTester() {
    bindButtons();
    bindUploads();
    onSocketReady.push(initWebsocket);
    onGoogleReady.push(getAllTests);
    onGoogleReady.push(getAttemptsLeft);
    console.info("Initialized tester script!");
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", async (e) => {
        e.preventDefault();
        await submit();
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

function initWebsocket(connection) {
    if(typeof connectionId === "undefined") {
        console.warn("Could not initialize WebSocket for the tester... is SignalR loaded?")
        return;
    }
    connection.on("ReceiveUpdate", (data) => {
        if (data == null)
            return;
        websocketBuffer.push(data);
        displayOutput(websocketBuffer.join('\n'));
    });
    console.log("Bound WebSocket!")
}

async function submit() {
    let data = new FormData();
    websocketBuffer = [];
    lastAnimatedBox = -1;

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
    if (typeof connectionId !== "undefined")
        data.append("ConnectionID", connectionId);

    document.getElementById("result").innerHTML = "Now loading...";
    document.getElementById("submit").disabled = true;
    document.getElementById("loading-icon").classList.remove("hidden");

    try {
        let response = await fetch("/Tester/Upload", {
            method: "POST",
            body: data
        });
        if (response.status >= 400 && response.status < 600) {
            handleErrors(response.status, null);
        } else {
            let data = await response.text();
            if (!websocketBuffer.join('\n').includes(data)) {
                displayOutput(data);
            }
            // Enable the animation for the last box.
            document.getElementById("result").lastChild.style.transition = "";
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
    
    document.getElementById("submit").disabled = false;
    document.getElementById("loading-icon").classList.add("hidden");
    await getAttemptsLeft();
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

            if (i >= processed.length - 2) {
                button.classList.toggle("active");
            }

            output.appendChild(button);
            output.appendChild(content);
        }

        content.innerHTML += item;
        if (button.classList.contains("active")) {
            if (lastAnimatedBox < i)
                lastAnimatedBox = i;
            else
                content.style.transition = "none";
            content.style.maxHeight = content.scrollHeight + "px";
        }
    }

    registerCollapsibles();
}

async function getTest(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    try {
        let response = await fetch("/Tester/GetTest", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "TestID": id
            })
        });

        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            let data = await response.json();
            if (!data.hasOwnProperty("requiredFiles")) {
                console.error("Could not find the required files from tester!");
                return;
            }
            generateInputs(data);
            bindUploads();
            currentTest = id;
        }
    } catch (e) {
        handleErrors(0, e);
    }
}

async function getAllTests() {
    clearInputs();
    try {
        let response = await fetch("/Tester/GetAllTests", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken()
            })
        })
        if (response.status >= 400 && response.status < 600) {
            handleErrors(response.status, null);
        } else {
            let data = await response.json();
            generateList(data);
            bindUploads();
        }
    } catch (e) {
        handleErrors(0, e);
    }
}

async function getAttemptsLeft() {
    try {
        let response = await fetch("/Users/GetTimeout", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(getLoginToken())
        });
        if (response.status >= 400 && response.status < 600) {
            handleErrors(response.status, null);
        } else {
            let data = await response.json();
            resetTime = new Date(Date.now() + data.milliseconds);
            testsRemaining = data.testsRemaining;
            updateAttemptsVisual();
            intervalTimer = setInterval(updateTimer, 1000);
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
}

function updateTimer() {
    // Oh no, timezones!
    let difference = resetTime - new Date();
    let output = document.getElementById("time");
    if (difference < -5) { // Delay to compensate for server time.
        output.innerHTML = "";
        updateAttemptsVisual();
        clearInterval(intervalTimer);
        intervalTimer = 0;
        return;
    }
    if (difference < 0) { // Actually clear the box.
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

function generateInputs(data) {
    generateIncluded(data.includedFiles);
    generateUploads(data.requiredFiles);
    document.querySelector(".test-info").innerHTML = data.description;
}

function generateUploads(uploads) {
    let inputArea = document.getElementById("fileInputs");
    for (let file of uploads) {
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
    document.querySelectorAll(".row .column")[1].classList.remove("not-ready");
}

function generateIncluded(included) {
    if (included === undefined || included == null)
        return;
    
    let includedArea = document.querySelector("#included");
    includedArea.querySelector(".row h4").innerHTML = "Included Files (" + included.length + ")";
    let includedList = includedArea.querySelector(".center");
    includedList.innerHTML = "";
    for (let file of included) {
        let button = document.createElement("button");
        button.className = "input normal";
        button.innerHTML = file;
        button.addEventListener("click", async (e) => {
            e.preventDefault();
            await downloadFile(file);
        });
        includedList.appendChild(button);
    }
}

async function downloadFile(file) {
    try {
        let response = await fetch("/Tester/Download", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "TestID": currentTest,
                "File": file
            })
        });
        if (response.status >= 400 && response.status < 600) {
            handleErrors(response.status, null);
        } else {
            // What's compatibility? And can I eat it?
            const newBlob = new Blob([ await response.blob() ], { type: response.headers.get("Content-Type") });

            if (window.navigator && window.navigator.msSaveOrOpenBlob)
                window.navigator.msSaveOrOpenBlob(newBlob);
            else {
                const data = URL.createObjectURL(newBlob);
                /* const newPanel = open(data, "_blank");
                if (newPanel !== null)
                    newPanel.focus(); */

                let link = document.createElement("a");
                link.href = data;
                link.download = file;
                link.click();
                
                // I saw this somewhere, apparently for Firefox.
                setTimeout(() => { URL.revokeObjectURL(data); }, 250);
            }
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
}

function generateList(tests) {
    let list = document.getElementById("tests-available");
    for(let test of tests) {
        let testBtn = document.createElement("button");
        testBtn.className = "default form-control";
        testBtn.innerHTML = test.name;
        testBtn.name = test.id;
        testBtn.addEventListener("click", async (e) => {
            await getTest(e.target.name);
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