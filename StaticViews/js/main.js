"use strict";

export { triggerPopup, triggerPopupButtons, disablePopup, registerCollapsibles, getUrl, handleErrors, getErrorMessage }

onInit();

function onInit() {
    fixNavbar();
    fixFooter();
    console.info("Initialized main script!");
    readError();
    fixPopup();
    if (!checkForCookieSupport())
        triggerPopup("Mukyu~", "Cookies need to be enabled for login to work!");
}

function fixNavbar() {
    let height = document.getElementsByTagName("nav")[0].offsetHeight;
    // Sometimes on reload, the browser reports wrong values (cached).
    document.getElementsByClassName("nav-padding")[0].style.height = Math.min(height, 70) + "px";
}

function fixFooter() {
    let height = document.getElementsByTagName("footer")[0].offsetHeight;
    document.getElementsByClassName("footer-padding")[0].style.height = height + "px";
}

function fixPopup() {
    document.querySelectorAll('[modulr-trigger="disablePopup"]').forEach(o => o.addEventListener("click", (e) => {
        e.preventDefault();
        disablePopup();
    }));
    document.querySelector(".blocker").addEventListener("click", (e) => {
        e.preventDefault();
        disablePopup();
    });
    document.querySelector(".blocker-message").addEventListener("click", (e) => {
        e.stopPropagation();
    });
}

function registerCollapsibles() {
    let buttons = document.getElementsByClassName("collapse");

    for (let button of buttons) {
        button.addEventListener("click", function() {
            this.classList.toggle("active");
            let content = this.nextElementSibling;
            if (content.style.maxHeight)
                content.style.maxHeight = null;
            else
                content.style.maxHeight = content.scrollHeight + "px";
        });
    }
}

function getUrl(urlLink, params) {
    let url = new URL(window.location.origin + urlLink);
    Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
    return url;
}

function triggerPopup(header, message) {
    let blocker = document.getElementsByClassName("blocker")[0];
    blocker.style.height = null; // Hm.
    document.getElementById("blocker-header").innerHTML = header;
    document.getElementById("blocker-message").innerHTML = message;
    blocker.getElementsByClassName("blocker-buttons")[0].classList.add("hidden");
    blocker.className = "blocker blocker-on";
    // blocker.addEventListener("click", disablePopup)
    document.getElementsByClassName("blocker-message")[0].className = "blocker-message blocker-message-on";
}

function triggerPopupButtons(buttons) {
    let blockerButtons = document.getElementsByClassName("blocker")[0].getElementsByClassName("blocker-buttons")[0];
    if(buttons === undefined || buttons == null || !Array.isArray(buttons)) {
        blockerButtons.classList.add("hidden");
        return;
    }
    blockerButtons.innerHTML = "";
    let cancelButton = document.createElement("button");
    cancelButton.className = "default";
    cancelButton.onclick = disablePopup;
    cancelButton.innerHTML = "Cancel";
    blockerButtons.appendChild(cancelButton);
    for (let button of buttons) {
        blockerButtons.appendChild(button);
        button.addEventListener("click", (e) => {
            e.preventDefault();
            disablePopup();
        }, false);
    }
    blockerButtons.classList.remove("hidden");
}

function disablePopup() {
    document.getElementsByClassName("blocker")[0].className = "blocker blocker-off";
    document.getElementsByClassName("blocker-message")[0].className = "blocker-message blocker-message-off";
}

function readError() {
    let params = new URLSearchParams(window.location.search);
    if (params.has("error"))
        triggerPopup("Mukyu~", decodeURIComponent(params.get("error")))
}

function checkForCookieSupport() {
    // https://stackoverflow.com/questions/6663859/check-if-cookies-are-enabled
    if (navigator.cookieEnabled) return true;
    document.cookie = "cookietest=1";
    let ret = document.cookie.indexOf("cookietest=") !== -1;
    document.cookie = "cookietest=1; expires=Thu, 01-Jan-1970 00:00:01 GMT";
    return ret;
}

function handleErrors(statusCode, error) {
    let httpError = getErrorMessage(statusCode);
    if (httpError != null && error == null)
        error = httpError;
    if (error != null) {
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    }
}

function getErrorMessage(statusCode) {
    let error = null;
    switch (statusCode) {
        case 400:
            error = "Input was invalid!";
            break;
        case 403:
            error = "Login credentials failed. Please try again.";
            break;
        case 404:
            error = "Could not locate whatever you were looking for, please try again!";
            break;
        case 413:
            error = "File was too large!";
            break;
        case 500:
        case 502:
        case 504:
            error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
            break;
    }
    
    return error;
}