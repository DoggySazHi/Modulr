﻿'use strict';

onInit();

function onInit() {
    bindButtons();
    bindUploads();
    fixNavbar();
    console.log("Initialized main script!");
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", (e) => {
        e.preventDefault();
        submit();
    }, false);

    document.getElementById("testId").addEventListener("change", (e) => {
        getTest(e.target.value);
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

function fixNavbar() {
    let height = document.getElementsByTagName("nav")[0].offsetHeight;
    document.getElementsByClassName("nav-padding")[0].style.height = height + "px";
}

function getUrl(urlLink, params) {
    let url = new URL(window.location.origin + urlLink);
    Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
    return url;
}

// stupid IDE not detecting the Google script
// noinspection JSUnusedGlobalSymbols
async function googleInit() {
    let result = await fetch("/Google/GetKey");
    let key = await result.json();
    
    gapi.load('auth2', function() {
        gapi.auth2.init(key).then(() => {
            renderLogin();
        });
    });
}

function renderLogin() {
    gapi.signin2.render('googleSignIn', {
        'scope': 'profile email',
        'width': 200,
        'height': 40,
        'longtitle': true,
        'theme': 'dark',
        'onsuccess': onSignIn,
        'onfailure': onSignInError
    });
}

function onSignIn(user)
{
    console.log('Logged in! User: ' + user.getBasicProfile().getName());
    let token = user.getAuthResponse().id_token;
    fetch("/Google/Login", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(token)
    })
    .then((response) => response.json())
    .then((message) => {
        if (!message.success)
            console.error("Server didn't like our Google login!\n" + message.error);
    });
}

function onSignInError(error)
{
    console.error("Failed to sign-in with Google...\n" + error);
}

function signOut() {
    var auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut().then(function () {
        console.log('Logged out!');
    });
}

function submit() {
    let data = new FormData();
    
    document.querySelectorAll("input[type='file']").forEach((input) => {
            data.append('FileNames', input.name);
            for (let i = 0; i < input.files.length; i++)
                data.append('Files', input.files[i]);
            data.append('IsTester', JSON.stringify(false));
            data.append('TestID', JSON.stringify(parseInt(document.getElementById("testId").value, 10)))
        }
    );

    document.getElementById("result").innerHTML = "Now loading...";
    
    fetch("/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTP Error " + response.status);
        return response.text();
    })
    .then((formatted) => {
        document.getElementById("result").innerHTML = formatted;
    })
    .catch((error) => {
        console.error("We had an error... ", error);
        document.getElementById("result").innerHTML = "There was an error. Of course there was.";
    });
}

function getTest(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    fetch(getUrl("/Tester/GetTest", { id: id }))
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("Server returned an HTTP Error: " + response.status);
        return response.json()
    })
    .then((formatted) => {
        if(!formatted.hasOwnProperty("RequiredFiles"))
            return;
        generateInputs(formatted.RequiredFiles);
        bindUploads();
    })
    .catch((error) => {
        console.error("We had an error... ", error);
        document.getElementById("result").innerHTML = error;
    });
}

function clearInputs() {
    let inputArea = document.getElementById("fileInputs");
    inputArea.innerHTML = "";
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
}