"use strict";

export { bindCaptcha, resetCaptcha };

let buttonCache = [];
let siteKey = null;

await onInitCaptcha();

async function onInitCaptcha() {
    console.log("Waiting for reCAPTCHA...");
    await waitForGoogle();
}

async function waitForGoogle() {
    if(typeof grecaptcha !== "undefined" && typeof grecaptcha.render !== "undefined")
        await captchaInit();
    else
        setTimeout(waitForGoogle, 100);
}

async function captchaInit() {
    let result = await fetch("/Google/GetCaptchaKey");
    siteKey = await result.text();

    for (let item of buttonCache)
        bindCaptcha(item.button, item.callback);
}

function bindCaptcha(button, callback) {
    if (siteKey == null) {
        buttonCache.push({ "button" : button, "callback" : callback });
        return;
    }

    grecaptcha.render(button, {
        "sitekey" : siteKey,
        "callback" : callback,
        "theme": "dark"
    });
    
    fixStyling();
}

function resetCaptcha() {
    grecaptcha.reset();
    fixStyling();
}

function fixStyling() {
    [".grecaptcha-logo iframe", ".grecaptcha-badge"].forEach((o) => {
        const item = document.querySelector(o);
        item.style.width = "0"; // Pichuun! We handle styling.
        item.style.height = "0";
        item.style.bottom = null;
        item.style.right = null;
    });
}