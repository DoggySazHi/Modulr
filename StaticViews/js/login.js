"use strict";

import { triggerPopup, triggerPopupButtons, disablePopup, registerCollapsibles, getUrl, handleErrors } from "./main.js"

import { onGoogleReady, onLoginEvent, renderLogin, signOut as signOutGoogle, getLoginToken } from "./google.js"

import { bindCaptcha, resetCaptcha } from "./captcha.js"

onInitLogin();

function onInitLogin() {
    onGoogleReady.push(checkSignIn);
    onLoginEvent.push(googleTrigger);
    console.info("Initialized login script!");
}

function signInPopup() {
    // I know, HTML in JavaScript, ew~
    const form = `
        <form class="row center">
            <label for="login-email">Email</label>
            <input type="email" id="login-email" />
            <label for="login-password">Password</label>
            <input type="password" id="login-password" />
            <div class="row center">
                <button class="normal">Sign In</button>
                <button class="default">Reset Password</button>
            </div>
            <div>(or use)</div>
            <div id="google-submit"></div>
        </form>
    `;
    
    let output = document.createElement("div");
    output.innerHTML = form;
    output.querySelector("button").addEventListener("click", (e) => {
        e.preventDefault();
    });
    
    triggerPopup("Welcome to Modulr!", output.innerHTML);
    
    renderLogin("google-submit");
}

function checkSignIn() {
    if (getLoginToken() !== undefined) {
        createSignOut();
    }
}

function googleTrigger(user) {
    document.getElementById("username").innerHTML = "Hello " + user.getBasicProfile().getName() + "!";
}

function createSignIn() {
    let button = document.getElementById("sign-in");
    button.innerHTML = "";
    let signInButton = document.createElement("button");
    signInButton.className = "button-compact success";
    signInButton.innerHTML = "Log In";
    signInButton.addEventListener("click", async (e) => {
        e.preventDefault();
        signInPopup();
    });
    button.appendChild(signInButton);
}

function createSignOut() {
    let button = document.getElementById("sign-in");
    button.innerHTML = "";
    let signOutButton = document.createElement("button");
    signOutButton.className = "button-compact danger";
    signOutButton.innerHTML = "Log Out";
    signOutButton.addEventListener("click", async (e) => {
        e.preventDefault();
        await signOut(true);
    });
    button.appendChild(signOutButton);
}


async function signOut(redirect) {
    await fetch(getUrl("/Users/LogOut", {}));
    await signOutGoogle();
    console.info('Logged out!');
    document.getElementById("sign-in").innerHTML = "";
    document.getElementById("username").innerHTML = "";
    if (redirect)
        window.location.replace(getUrl("/", {}));
}