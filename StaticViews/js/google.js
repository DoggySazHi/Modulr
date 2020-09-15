﻿'use strict';

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
        'width': 160,
        'height': 32,
        'longtitle': false,
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
            if (!message.success) {
                console.error("Server didn't like our Google login!\n" + message.error);
                signOut();
            } else {
                createSignOut();
            }
        });
}

function onSignInError(error)
{
    if (error.error !== "popup_closed_by_user")
        console.error("Failed to sign-in with Google...", error);
}

function createSignOut() {
    let button = document.getElementById("googleSignIn");
    button.innerHTML = "";
    let signOutButton = document.createElement("button");
    signOutButton.className = "button-compact danger";
    signOutButton.innerHTML = "Log Out";
    signOutButton.addEventListener("click", signOut);
    button.appendChild(signOutButton);
}

function signOut() {
    let google = gapi.auth2.getAuthInstance();
    google.signOut().then(function () {
        console.log('Logged out!');
        renderLogin();
    });
    document.getElementById("googleSignIn").innerHTML = "";
}