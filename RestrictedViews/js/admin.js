﻿"use strict";

onInitAdmin();

let currentTest = 0;
let startSwap;
let endSwap;

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
    document.getElementById("delete").addEventListener("click", (e) => {
        e.preventDefault();
        remove();
    }, false);
    document.getElementById("tester-add").addEventListener("click", (e) => {
        e.preventDefault();
        addTester();
    }, false);
    document.getElementById("required-add").addEventListener("click", (e) => {
        e.preventDefault();
        addRequired();
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
    let order = 1;
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

            let dragChar = document.createElement("span");
            dragChar.innerHTML = "\u2195";
            
            label.appendChild(dragChar)
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
            label.draggable = true;
            label.style.order = order + "";
            order++;
            label.addEventListener("dragstart", (e) => {
                startSwap = e.target;
            });
            label.addEventListener("dragover", (e) => {
                e.preventDefault();
            })
            label.addEventListener("drop", (e) => {
                if(e.target !== startSwap) {
                    for (let label of document.getElementById("testers").children) {
                        if (label === e.target || label.contains(e.target)) {
                            let oldOrder = label.style.order;
                            label.style.order = startSwap.style.order;
                            startSwap.style.order = oldOrder;
                        }
                    }
                }
                startSwap = null;
            })
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
    list.innerHTML = "";
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
    let message = ["Now uploading files, if any..."];
    triggerPopup("Updating...", message.join('\n'));
    document.getElementById("submit").disabled = true;

    let data = new FormData();

    let fileInputs = document.querySelectorAll("input[type='file']");
    for (let input of fileInputs) {
        if(input.files.length === 0)
            continue;
        data.append('FileNames', input.name);
        data.append('Files', input.files[0]);
        data.append('IsTester', JSON.stringify(false));
        data.append('TestID', JSON.stringify(currentTest));
        data.append('AuthToken', getLoginToken());
    }

    fetch("/Admin/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR " + response.status);
        return response.text();
    })
    .then((formatted) => {
        let text = formatted.toString();
        message.push("---");
        message.push(text);
        message.push("---\nNow updating stipulatable information...");
        triggerPopup("Updating...", message.join('\n'));
        updateStipulatable(message);
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 400:
                    error = "The server didn't like your files... did you miss something?";
                    break;
                case 403:
                    error = "Either you are on a cooldown, or your login credentials failed.\nIf the former isn't true, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not locate the uploader... try refreshing the page?";
                    break;
                case 500:
                case 502:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
            document.getElementById("submit").disabled = false;
        }

        message.push(error);
        triggerPopup("Mukyu~", message.join('\n'));
    });
}

function updateStipulatable(message) {
    fetch("/Admin/Tester/Update", {
        method: "PUT",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "AuthToken": getLoginToken(),
            "TestID": currentTest,
            "TestName": document.querySelector("input[autocorrect]").value,
            // William, what the heck? (Gets all children, extracts name from each input element)
            "Required": [...document.getElementById("required").children].map(o => o.getElementsByTagName("input")[0].value),
            // ... (Get all children under the testers, sort by flexbox order for draggables, then grab the names of each tester)
            "Testers": [...document.getElementById("testers").children].sort((p, q) => parseInt(p.style.order) - parseInt(q.style.order)).map(o => o.getElementsByTagName("input")[1].value)
        })
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        if(!formatted)
            message.push("Server failed to find a record; was it deleted?");
        else
            message.push("Successfully updated the stipulatable's information!\nDone!");
        triggerPopup("Finished updating!", message.join('\n'));
        getAllTestsAdmin();
        getTestAdmin(currentTest);
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
        message.push(error);
        triggerPopup("Error updating!", message.join('\n'));
    }).finally(() => {
        document.getElementById("submit").disabled = false;
    });
}

function remove() {
    let name = document.querySelector("input[autocorrect]").value;
    let deleteBtn = document.createElement("button");
    deleteBtn.className = "danger form-control";
    deleteBtn.innerHTML = "Delete";
    deleteBtn.addEventListener("click", (e) => {
        e.preventDefault();
        actuallyDelete();
    }, false);
    triggerPopup("Delete?", "Are you sure you want to delete the test for \"" + name + "\" (id " + currentTest + ")?");
    triggerPopupButtons([deleteBtn]);
}

function actuallyDelete() {
    clearInputs();
    fetch("/Admin/Tester/Delete", {
        method: "DELETE",
        headers: {
            'Content-Type': 'application/json'
        },
        body: currentTest
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR" + response.status);
        return response.json();
    })
    .then((formatted) => {
        if(!formatted)
            triggerPopup("Mukyu~", "The server couldn't delete anything! Is the cache old?");
        getAllTestsAdmin();
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "The test was not found... was it deleted already?";
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

function addTester() {
    let inputArea = document.getElementById("testers")

    let order = [...document.getElementById("testers").children].sort((p, q) =>  parseInt(q.style.order) - parseInt(p.style.order));
    order = order.length === 0 ? 0 : order[0].style.order; // If there are no items, the first order is zero, otherwise it's the greatest order (by sort).
    
    let label = document.createElement("label");
    let labelName = document.createElement("input");
    label.className = "input";
    let removeButton = document.createElement("button");
    let input = document.createElement("input");
    input.type = "file";

    let dragChar = document.createElement("span");
    dragChar.innerHTML = "\u2195";

    label.appendChild(dragChar)
    label.appendChild(input);
    label.appendChild(labelName);

    let statusChar = document.createElement("span");
    label.classList.add("normal");
    statusChar.innerHTML = "?";
    label.appendChild(statusChar);
    label.draggable = true;
    label.style.order = order + "";
    label.addEventListener("dragstart", (e) => {
        startSwap = e.target;
    });
    label.addEventListener("dragover", (e) => {
        e.preventDefault();
    })
    label.addEventListener("drop", (e) => {
        if(e.target !== startSwap) {
            for (let label of document.getElementById("testers").children) {
                if (label === e.target || label.contains(e.target)) {
                    let oldOrder = label.style.order;
                    label.style.order = startSwap.style.order;
                    startSwap.style.order = oldOrder;
                }
            }
        }
        startSwap = null;
    })

    removeButton.className = "danger modifier-btn";
    removeButton.innerHTML = "&minus;";
    removeButton.addEventListener("click", (e) => {
        e.preventDefault();
        e.target.parentElement.remove();
    });
    label.appendChild(removeButton);
    inputArea.appendChild(label);
}

function addRequired() {
    let inputArea = document.getElementById("required")
    let label = document.createElement("label");
    let labelName = document.createElement("input");
    label.className = "input";
    let removeButton = document.createElement("button");
    
    let input = document.createElement("div");
    label.appendChild(input);

    label.classList.add("normal");

    label.appendChild(labelName);

    removeButton.className = "danger modifier-btn";
    removeButton.innerHTML = "&minus;";
    removeButton.addEventListener("click", (e) => {
        e.preventDefault();
        e.target.parentElement.remove();
    });
    label.appendChild(removeButton);
    inputArea.appendChild(label);
}