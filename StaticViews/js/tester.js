'use strict';

onInit();

function onInit() {
    bindButtons();
    bindUploads();
    fixNavbar();
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", (e) => {
        e.preventDefault();
        submit();
    }, false);
}

function bindUploads() {
    document.querySelectorAll("input[type='file']").forEach((input) => {
        input.addEventListener("change", (e) => {
            console.log("Changed");
            if (e.target.value === "")
                e.target.parentNode.className = "input normal";
            else
                e.target.parentNode.className = "input success";
        }, false);
    });
}

function fixNavbar() {
    let height = document.getElementsByTagName("nav")[0].offsetHeight;
    document.getElementsByClassName("nav-padding")[0].style.height = height + "px";
}

function getUrl(urlLink, params) {
    let url = new URL(urlLink);
    Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
}

function submit() {
    let data = new FormData();
    
    document.querySelectorAll("input[type='file']").forEach((input) => {
            data.append('FileNames', input.name);
            for (let i = 0; i < input.files.length; i++)
                data.append('Files', input.files[i]);
            // bruh
            data.append('IsTester', JSON.stringify(false));
        }
    );

    document.getElementById("result").innerHTML = "Now loading...";
    
    fetch("/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => response.text())
    .then((formatted) => {
        document.getElementById("result").innerHTML = formatted;
    })
    .catch((error) => {
        console.error("We had an error... ", error);
        document.getElementById("result").innerHTML = "There was an error. Of course there was.";
    });
}

function getTest() {
    let id = parseInt(document.getElementById("testId").innerHTML, 10);
    if (isNaN(id))
        return;
    let url = getUrl("/Tester/GetTest", { id: 0 })
    fetch("/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => response.text())
    .then((formatted) => {
        document.getElementById("result").innerHTML = formatted;
    })
    .catch((error) => {
        console.error("We had an error... ", error);
        document.getElementById("result").innerHTML = "There was an error. Of course there was.";
    });
}