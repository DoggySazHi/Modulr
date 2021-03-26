"use strict";

import {getLoginToken, onGoogleReady} from "/js/google.js";
import {triggerPopup, triggerPopupButtons, handleErrors} from "/js/main.js";

let allUsers = [];
let currentUser = -1;

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
    document.getElementById("resetTimeout").addEventListener("click", async (e) => {
        e.preventDefault();
        await resetTimeout();
    }, false);
    document.getElementById("update").addEventListener("click", async (e) => {
        e.preventDefault();
        await updateUserInfo();
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
    let list = document.getElementById("users");
    let manager = document.getElementById("manager");
    
    manager.classList.add("hidden");
    list.innerHTML = "";
    
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
    
    manager.classList.remove("hidden");
}

function loadUserInfo(id) {
    id = parseInt(id);
    
    let output = document.getElementById("usermod");
    let record = allUsers.find((o) => o.id === id);
    if (record === undefined || record == null) {
        failUserNotFound(id);
        return;
    }
    currentUser = id;
    
    output.classList.add("hidden");
    
    document.getElementById("usernameMod").innerHTML = record.username;
    document.getElementById("emailMod").innerHTML = record.email;
    document.getElementById("modulrID").innerHTML = record.id;
    document.getElementById("googleID").innerHTML = record.googleID;
    document.getElementById("name").value = record.name;
    document.getElementById("remainingTestsMod").innerHTML = record.testsRemaining;
    let resetDate = new Date(record.testsTimeout);
    let diff = (resetDate - new Date()) / 1000;
    if (diff <= 0)
        diff = 0;
    let hours = Math.floor(diff / 60 / 60);
    let minutes = Math.floor(diff / 60) - hours * 60;
    let seconds = Math.round(diff) - minutes * 60 - hours * 60 * 60;
    
    document.getElementById("timeLeftMod").innerHTML = hours + ":" + ("" + minutes).padStart(2, "0") + ":" + ("" + seconds).padStart(2, "0");
    document.getElementById("resetTimeMod").innerHTML = resetDate.toLocaleString();

    document.getElementById("admin").checked = (record.role & 1) === 1;
    document.getElementById("banned").checked = (record.role & 2) === 2;

    output.classList.remove("hidden");
}

async function resetTimeout() {
    try {
        let record = allUsers.find((o) => o.id === currentUser);
        if (record === undefined || record == null) {
            failUserNotFound(currentUser);
            return;
        }
        let response = await fetch("/Admin/System/ResetUserTimeout", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "id": currentUser,
                "googleID": record.googleID,
                "email": record.email,
                "username": record.username,
                "name": document.getElementById("name").value,
                "role": (document.getElementById("admin").checked ? 1 : 0) +
                    (document.getElementById("banned").checked ? 2 : 0)
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        triggerPopup("Updated " + record.name + "!", "Modulr has reset " + record.name + "'s test counter.");
        triggerPopupButtons(null);
        await populateUsers();
        await loadUserInfo(currentUser);
    }
    catch (e) {
        handleErrors(0, e);
    }
}

async function updateUserInfo() {
    try {
        let record = allUsers.find((o) => o.id === currentUser);
        if (record === undefined || record == null) {
            failUserNotFound(currentUser);
            return;
        }
        let response = await fetch("/Admin/System/UpdateUser", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "id": currentUser,
                "googleID": record.googleID,
                "email": record.email,
                "username": record.username,
                "name": document.getElementById("name").value,
                "role": (document.getElementById("admin").checked ? 1 : 0) +
                    (document.getElementById("banned").checked ? 2 : 0)
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        triggerPopup("Updated " + record.name + "!", "Modulr has updated the user's information.");
        triggerPopupButtons(null);
        await populateUsers();
        await loadUserInfo(currentUser);
    }
    catch (e) {
        handleErrors(0, e);
    }
}

function failUserNotFound(id) {
    let error = "Cannot find user with ID " + id + "; refresh?";
    console.error(error);
    triggerPopup("Mukyu~", error);
    triggerPopupButtons(null);
}