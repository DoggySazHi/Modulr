"use strict";

onInit();

function onInit() {
    fixNavbar();
    fixFooter();
    console.info("Initialized main script!");
    readError();
    if (!checkForCookieSupport())
        triggerPopup("Mukyu~", "Cookies need to be enabled for login to work!")
}

function fixNavbar() {
    let height = document.getElementsByTagName("nav")[0].offsetHeight;
    document.getElementsByClassName("nav-padding")[0].style.height = height + "px";
}

function fixFooter() {
    let height = document.getElementsByTagName("footer")[0].offsetHeight;
    document.getElementsByClassName("footer-padding")[0].style.height = height + "px";
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
    document.getElementById("blocker-header").innerHTML = header;
    document.getElementById("blocker-message").innerHTML = message;
    blocker.className = "blocker blocker-on";
    // blocker.addEventListener("click", disablePopup)
    document.getElementsByClassName("blocker-message")[0].className = "blocker-message blocker-message-on";
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