"use strict";

import { triggerPopup, disablePopup, getUrl, handleErrors, getErrorMessage } from "./main.js"

import { onLoginEvent, renderLogin, signOut as signOutGoogle } from "./google.js"

import { bindCaptcha, resetCaptcha } from "./captcha.js"

await onInitLogin();

async function onInitLogin() {
    await checkSignIn();
    onLoginEvent.push(googleTrigger);
    console.info("Initialized login script!");
}

function signInPopup() {
    // I know, HTML in JavaScript, ew~
    const form = `
        <form class="row center" onsubmit="return false;">
            <label for="login-name" class="show-register">Name</label>
            <div class="show-register muted">At least three characters long.</div>
            <input type="text" id="login-name" class="show-register" />
            <label for="login-email">Email</label>
            <input type="email" id="login-email" />
            <label for="login-password">Password</label>
            <div class="show-register muted">At least five characters long.</div>
            <input type="password" id="login-password" />
            <label for="login-password-again" class="show-register">Repeat Password</label>
            <input type="password" id="login-password-again" class="show-register" />
            <div id="login-message"></div>
            <div class="row center">
                <button class="normal login-btn"></button>
                <button class="default register-btn"></button>
            </div>
            <div class="login-divider">(or use)</div>
            <div id="google-submit"></div>
        </form>
    `;
    
    let output = document.createElement("div");
    output.innerHTML = form;
    
    triggerPopup("Welcome to Modulr!", output.innerHTML);
    
    renderLogin("google-submit");
    
    let submitBtn = document.querySelector(".login-btn");
    bindCaptcha(submitBtn, onSubmit);
    
    document.querySelector(".register-btn").addEventListener("click", (e) => {
        e.preventDefault();
        document.querySelector("#blocker-message form").classList.toggle("register");
    });
}

async function checkSignIn() {
    let user = await getCurrentUser();
    document.getElementById("sign-in").innerHTML = "";

    const username = document.getElementById("username");
    const navContainer = document.querySelector(".nav-container");
    const navDivider = document.querySelector(".nav-item-right");
    
    if (user == null) {
        createSignIn();
        username.innerHTML = "";
        
        ["student-test", "admin", "admin-system"].forEach((o) => {
            const item = document.getElementById(o);
            if (item !== undefined && item != null)
                item.remove();
        });
    } else {
        createSignOut();
        username.innerHTML = "Hello " + user.name + "!";
        
        const studentLink = document.createElement("a");
        studentLink.innerHTML = `<span class="nav-item" id="student-test">Student Test</span>`
        studentLink.href = "/tester/student-test";

        const adminLink = document.createElement("a");
        adminLink.innerHTML = `<span class="nav-item" id="admin">Test Manager</span>`
        adminLink.href = "/admin";

        const adminLink2 = document.createElement("a");
        adminLink2.innerHTML = `<span class="nav-item" id="admin-system">System Manager</span>`
        adminLink2.href = "/admin/system";
        
        navContainer.insertBefore(studentLink, navDivider);
        if ((user.role & 1) === 1) {
            navContainer.insertBefore(adminLink, navDivider);
            navContainer.insertBefore(adminLink2, navDivider);
        }
    }
}

async function googleTrigger(ignored) {
    await signOutGoogle(); // Because we no longer need the Google login; the Modulr cookie holds it for us.
    disablePopup();
    await checkSignIn();
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
    await fetch("/Users/LogOut", { method : "POST" });
    await signOutGoogle();
    console.info('Logged out!');
    document.getElementById("sign-in").innerHTML = "";
    document.getElementById("username").innerHTML = "";

    if (redirect)
        window.location.replace(getUrl("/", {}));
    else
        await checkSignIn();
}

async function onSubmit(captcha) {
    const isRegisterMode = document.querySelector("#blocker-message form").classList.contains("register");
    if (isRegisterMode)
        await registerModulr(captcha);
    else
        await loginModulr(captcha);
}

async function loginModulr(captcha) {
    resetCaptcha();
    
    const email = document.getElementById("login-email");
    const password = document.getElementById("login-password");
    const messageBox = document.getElementById("login-message");
    try {
        let response = await fetch("/Users/Login", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "CaptchaToken": captcha,
                "Email": email.value,
                "Password": password.value
            })
        });
        if (response.status >= 400 && response.status < 600) {
            messageBox.className = "danger-text";
            messageBox.innerHTML = getErrorMessage(response.status, null);
            password.value = "";
            return;
        }
        disablePopup();
        await checkSignIn();
    }
    catch (e) {
        messageBox.className = "danger-text";
        messageBox.innerHTML = e;
    }
}

async function registerModulr(captcha) {
    resetCaptcha();
    const name = document.getElementById("login-name");
    const email = document.getElementById("login-email");
    const passwordA = document.getElementById("login-password");
    const passwordB = document.getElementById("login-password-again");
    const messageBox = document.getElementById("login-message");
    
    if (passwordA.value !== passwordB.value) {
        messageBox.innerHTML = "Your passwords do not match!";
        return;
    }
    
    try {
        let response = await fetch("/Users/Register", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "CaptchaToken": captcha,
                "Name": name.value,
                "Email": email.value,
                "Password": passwordA.value
            })
        });
        
        const message = await response.text();
        
        if (response.status >= 400 && response.status < 600) {
            messageBox.className = "danger-text";
            messageBox.innerHTML = getErrorMessage(response.status, message);
            return;
        }

        messageBox.className = "success-text";
        messageBox.innerHTML = "Registration successful. You may login.";
        clearInputs();

        document.querySelector("#blocker-message form").classList.remove("register");
    }
    catch (e) {
        messageBox.className = "danger-text";
        messageBox.innerHTML = e;
    }
}

function clearInputs() {
    ["login-name", "login-email", "login-password", "login-password-again"].forEach((o) => { document.getElementById(o).value = ""; });
}

async function getCurrentUser() {
    // No actual error handling is done here. If it's invalid, we just assume they're not logged in.
    try {
        let response = await fetch("/Users/GetCurrentUser");
        if (response.status >= 400 && response.status < 600)
            return null;
        return await response.json();
    }
    catch (e) {
       return null;
    }
}