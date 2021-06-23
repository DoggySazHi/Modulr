"use strict";

import { triggerPopup, getUrl } from "/js/main.js";

export { onLoginEvent, onGoogleReady, getLoginToken, renderLogin, signOut };

let onLoginEvent = [];
let onGoogleReady = [];

await onInitGoogle();

async function onInitGoogle() {
    console.log("Waiting for Google...");
    await waitForGoogle();
}

async function waitForGoogle() {
    if(typeof gapi !== "undefined")
        await googleInit();
    else
        setTimeout(waitForGoogle, 100);
}

async function googleInit() {
    let result = await fetch("/Google/GetKey");
    let key = await result.json();

    gapi.load('auth2', function() {
        gapi.auth2.init(key).then(() => {
            for (let f of onGoogleReady)
                f(gapi.auth2.getAuthInstance().currentUser.get());
        });
    });
}

function renderLogin(field) {
    gapi.signin2.render(field, {
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
        
        return false;
    } else {
        document.getElementById("username").innerHTML = "Hello " + user.getBasicProfile().getName() + "!";
    }
    
    for (let f of onLoginEvent)
        f(gapi.auth2.getAuthInstance().currentUser.get());
    
    return true;
}

function onSignInError(error)
{
    if (error.error !== "popup_closed_by_user")
        console.error("Failed to sign-in with Google...", error);
}

async function signOut() {
    let google = gapi.auth2.getAuthInstance();
    await google.signOut();
}

function getLoginToken() {
    return gapi.auth2.getAuthInstance().currentUser.get().getAuthResponse().id_token;
}