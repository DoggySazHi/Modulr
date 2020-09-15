'use strict';

onInit();

function onInit() {
    fixNavbar();
    console.log("Initialized main script!");
}

function fixNavbar() {
    let height = document.getElementsByTagName("nav")[0].offsetHeight;
    document.getElementsByClassName("nav-padding")[0].style.height = height + "px";
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