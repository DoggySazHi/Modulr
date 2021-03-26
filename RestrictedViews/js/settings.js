"use strict";

import {getLoginToken, onGoogleReady} from "/js/google.js";
import {triggerPopup, triggerPopupButtons, handleErrors} from "/js/main.js";

let allUsers = [];

await onInitSettings();

async function onInitSettings() {
    bindButtons();
    onGoogleReady.push(populateUsers);
    console.info("Initialized settings script!");
}

function bindButtons() {
    document.getElementById("rebuild").addEventListener("click", async (e) => {
        e.preventDefault();
        await rebuild();
    }, false);
    document.getElementById("shutdown").addEventListener("click", async (e) => {
        e.preventDefault();
        await shutdownWarning();
    }, false);
}

async function rebuild() {
    try {
        triggerPopup("Asking server to rebuild Docker image...", "Please wait warmly...");
        let response = await fetch("/Admin/System/RebuildContainer", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken()
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            let data = await response.text();
            triggerPopup("Rebuild done!", data);
            triggerPopupButtons(null);
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
}

async function shutdownWarning() {
    triggerPopup("Shut down warning!", "Modulr will shut down; whether it restarts is dependent on the computer's configuration. " +
        "Are you sure you want to continue?")
    let discardBtn = document.createElement("button");
    discardBtn.className = "danger form-control";
    discardBtn.innerHTML = "Shut down";
    discardBtn.addEventListener("click", async (e) => {
        e.preventDefault();
        await shutdown();
    }, false);
    triggerPopupButtons([discardBtn]);
}

async function shutdown() {
    try {
        let response = await fetch("/Admin/System/Shutdown", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken()
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        triggerPopup("Shut down successful!", "Modulr has shut down.");
        triggerPopupButtons(null);
    }
    catch (e) {
        handleErrors(0, e);
    }
}

async function populateUsers() {
    try {
        let response = await fetch("/Admin/System/GetAllUsers", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken()
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            allUsers = await response.json();
            addUsersToList();
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
}

function addUsersToList() {
    console.log(allUsers);
    let list = document.getElementById("users");
    console.log(list);
    for(let user of allUsers) {
        let userBtn = document.createElement("button");
        userBtn.className = "default form-control";
        userBtn.innerHTML = user.name;
        userBtn.name = user.id;
        userBtn.addEventListener("click", async (e) => {
            loadUserInfo(e.target.name);
        })
        list.appendChild(userBtn);
    }
    document.getElementById("manager").classList.remove("hidden");
}

function loadUserInfo(id) {
    let output = document.getElementById("usermod");
    output.classList.add("hidden");
    
    document.createElement("")
    
    output.classList.remove("hidden");
}