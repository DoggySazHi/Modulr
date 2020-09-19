'use strict';

onInitAuth();

function onInitAuth() {
    onLoginEvent.push(onLoginForceAuth);
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