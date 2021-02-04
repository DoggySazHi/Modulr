"use strict";

import {onGoogleReady} from "./google.js";
import {getUrl} from "./main.js";

onInitAuth();

function onInitAuth() {
    onGoogleReady.push(onLoginForceAuth);
    console.info("Waiting for Google to init...");
}

function onLoginForceAuth(user) {
    console.info("Verifying login...");
    if (!user.isSignedIn()) {
        console.error("Not logged in! Kicking to main menu.");
        window.location.replace(getUrl("/", {
            "error": "Login is required to be on that page!"
        }));
    }
}