"use strict";

import { triggerPopup, getUrl } from "/js/main.js";

export { onLoginEvent, onGoogleReady, getLoginToken };

let onLoginEvent = [];
let onGoogleReady = [];

// noinspection JSIgnoredPromiseFromCall
onInitGoogle();

async function onInitGoogle() {
    console.log("Waiting for Google...");
    await waitForGoogle();
}

async function waitForGoogle() {
    if(typeof gapi.signin2 !== "undefined")
        await googleInit();
    else
        setTimeout(waitForGoogle, 250);
}

async function googleInit() {
    let result = await fetch("/Google/GetKey");
    let key = await result.json();

    gapi.load('auth2', function() {
        gapi.auth2.init(key).then(() => {
            renderLogin();
            for (let f of onGoogleReady)
                f(gapi.auth2.getAuthInstance().currentUser.get());
        });
    });
}

async function renderLogin() {
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

async function onSignIn(user)
{
    console.info('Logged in! User: ' + user.getBasicProfile().getName());
    let token = user.getAuthResponse().id_token;
    let response = await fetch("/Google/Login", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(token)
    });
    let message = await response.json();
    if (!message.success) {
        triggerPopup("Mukyu~", "The server didn't let us login.\nMessage: " + message.error);
        console.error("Server didn't like our Google login!\n" + message.error);
        await signOut();
    } else {
        document.getElementById("username").innerHTML = "Hello " + user.getBasicProfile().getName() + "!";
        createSignOut();
    }
    for (let f of onLoginEvent)
        f(gapi.auth2.getAuthInstance().currentUser.get());
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

async function signOut() {
    let google = gapi.auth2.getAuthInstance();
    await fetch(getUrl("/Users/LogOut", {})).then(() => {
        google.signOut().then(function () {
            console.info('Logged out!');
            renderLogin();
            window.location.replace(getUrl("/", {}));
        });
    })
    document.getElementById("googleSignIn").innerHTML = "";
    document.getElementById("username").innerHTML = "";
}

function getLoginToken() {
    return gapi.auth2.getAuthInstance().currentUser.get().getAuthResponse().id_token;
}