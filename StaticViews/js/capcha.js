"use strict";

export { bindCapcha };

let buttonCache = [];
let siteKey = null;

await onInitCapcha();

async function onInitCapcha() {
    console.log("Waiting for reCAPCHA...");
    await waitForGoogle();
}

async function waitForGoogle() {
    if(typeof grecaptcha !== "undefined" && typeof grecaptcha.render !== "undefined")
        await capchaInit();
    else
        setTimeout(waitForGoogle, 100);
}

async function capchaInit() {
    let result = await fetch("/Google/GetCapchaKey");
    siteKey = await result.text();

    for (let item of buttonCache)
        bindCapcha(item.button, item.callback);
}

function bindCapcha(button, callback) {
    if (siteKey == null) {
        buttonCache.push({ "button" : button, "callback" : callback });
        return;
    }

    grecaptcha.render(button, {
        "sitekey" : siteKey,
        "callback" : callback,
        "theme": "dark"
    });
}