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
                e.target.parentNode.className = "input normal";
            else
                e.target.parentNode.className = "input success";
        }, false);
    });
    document.getElementById("submit").classList.remove("hidden");
}

function clearInputs() {
    let inputArea = document.getElementById("fileInputs");
    inputArea.innerHTML = "";
    let submit = document.getElementById("submit");
    submit.disabled = true;
    submit.classList.add("hidden");
}

function generateFileExplorer(formatted) {
    let inputArea = document.getElementById("fileInputs");
    let testerTitle = document.createElement("span");
    testerTitle.innerHTML = "Testers"
    testerTitle.className = "topic"
    let bar = document.createElement("hr");
    let requiredTitle = document.createElement("span");
    requiredTitle.innerHTML = "Required"
    requiredTitle.className = "topic"

    let testerRow = document.createElement("div");
    testerRow.className = "row center"
    let testerAddButton = document.createElement("button");
    testerAddButton.className = "success modifier-btn";
    testerAddButton.innerHTML = "&plus;";
    testerRow.appendChild(testerTitle);
    testerRow.appendChild(testerAddButton);

    let requiredRow = document.createElement("div");
    requiredRow.className = "row center"
    let requiredAddButton = document.createElement("button");
    requiredAddButton.className = "success modifier-btn";
    requiredAddButton.innerHTML = "&plus;";
    requiredRow.appendChild(requiredTitle);
    requiredRow.appendChild(requiredAddButton);

    inputArea.appendChild(testerRow);
    generateInputs(formatted.testerFiles);
    inputArea.appendChild(bar);
    inputArea.appendChild(requiredRow);
    generateInputs(formatted.requiredFiles);
}

function generateInputs(names) {
    let inputArea = document.getElementById("fileInputs");
    for (let file of names) {
        let label = document.createElement("label");
        label.className = "input";
        let removeButton = document.createElement("button");
        let input;
        
        if(file.hasOwnProperty("exists")) {
            input = document.createElement("input");
            input.type = "file";

            input.name = file.file;
            label.innerHTML = file.file;
            
            if(!file.exists) {
                label.classList.add("danger");
                label.innerHTML += " &#10060;";
            }
            else {
                label.classList.add("success");
                label.innerHTML += " &#10004;";
            }
        }
        else {
            input = document.createElement("div");

            label.classList.add("normal");
            input.name = file;
            removeButton.name = file;
            label.innerHTML = file;
        }
        
        removeButton.className = "danger modifier-btn";
        removeButton.innerHTML = "&minus;";
        removeButton.addEventListener("click", (e) => {
            e.preventDefault();
            e.target.parentElement.remove();
        });

        label.appendChild(input);
        label.appendChild(removeButton);
        inputArea.appendChild(label);
    }
    document.getElementById("submit").disabled = false;
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