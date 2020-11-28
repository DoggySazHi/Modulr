"use strict";

onInitAdmin();

let currentTest = 0;

function onInitAdmin() {
    bindButtons();
    bindUploads();
    onGoogleReady.push(getAllTestsAdmin);
    console.info("Initialized admin script!");
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
            if (e.target.value === "")
                e.target.parentNode.className = "input danger";
            else
                e.target.parentNode.className = "input success";
        }, false);
    });
    document.getElementById("submit").classList.remove("hidden");
}

function clearInputs() {
    document.getElementById("manager").classList.add("hidden");
    document.getElementById("testers").innerHTML = "";
    document.getElementById("required").innerHTML = "";
}

function generateFileExplorer(formatted) {
    let fileManager = document.getElementById("manager");
    fileManager.getElementsByTagName("input")[0].value = formatted.name;
    generateInputs(formatted.testerFiles, document.getElementById("testers"));
    generateInputs(formatted.requiredFiles, document.getElementById("required"));
    fileManager.classList.remove("hidden");
}

function generateInputs(names, inputArea) {
    for (let file of names) {
        let label = document.createElement("label");
        let labelName = document.createElement("input");
        label.className = "input";
        let removeButton = document.createElement("button");
        let input;
        
        if(file.hasOwnProperty("exists")) {
            input = document.createElement("input");
            input.type = "file";

            input.name = file.file;
            label.appendChild(input);
            label.appendChild(labelName);
            labelName.value = file.file;

            let statusChar = document.createElement("span");
            if(!file.exists) {
                label.classList.add("danger");
                statusChar.innerHTML = "&#10060;";
            }
            else {
                label.classList.add("success");
                statusChar.innerHTML = "&#10004;";
            }
            label.appendChild(statusChar);
        }
        else {
            input = document.createElement("div");
            label.appendChild(input);

            label.classList.add("normal");
            input.name = file;
            removeButton.name = file;

            label.appendChild(labelName);
            labelName.value = file;
        }
        
        removeButton.className = "danger modifier-btn";
        removeButton.innerHTML = "&minus;";
        removeButton.addEventListener("click", (e) => {
            e.preventDefault();
            e.target.parentElement.remove();
        });
        label.appendChild(removeButton);
        inputArea.appendChild(label);
    }
    document.getElementById("submit").disabled = false;
    document.getElementById("delete").disabled = false;
}

function generateList(tests) {
    let list = document.getElementById("tests-available");
    for(let test of tests) {
        let testBtn = document.createElement("button");
        testBtn.className = "default form-control";
        testBtn.innerHTML = test.name;
        testBtn.name = test.id;
        if(!test.valid) {
            testBtn.classList.add("danger");
            testBtn.innerHTML += " &#10060;";
        }
        else {
            testBtn.classList.add("normal");
            testBtn.innerHTML += " &#10004;";
        }
        testBtn.addEventListener("click", (e) => {
            getTestAdmin(e.target.name);
        })
        list.appendChild(testBtn);
    }
}

function getAllTestsAdmin() {
    clearInputs();
    fetch("/Admin/Tester/GetAll", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "AuthToken": getLoginToken()
        })
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        generateList(formatted);
        bindUploads();
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not fetch tests because none were found!";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    });
}

function getTestAdmin(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    fetch("/Admin/Tester/Get", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "AuthToken": getLoginToken(),
            "TestID": id
        })
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        if(!formatted.hasOwnProperty("testerFiles")) {
            console.error("Could not find the tester files from tester!");
            return;
        }
        if(!formatted.hasOwnProperty("requiredFiles")) {
            console.error("Could not find the required files from tester!");
            return;
        }
        
        generateFileExplorer(formatted);
        bindUploads();
        currentTest = id;
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not locate test, please try another one!";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    });
}

function submit() {
    
}