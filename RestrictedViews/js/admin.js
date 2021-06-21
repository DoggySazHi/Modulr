"use strict";

import {getLoginToken, onGoogleReady} from "/js/google.js";
import {triggerPopup, triggerPopupButtons, handleErrors} from "/js/main.js";

onInitAdmin();

let currentTest = 0;
let startSwap;
let modified = false;

function onInitAdmin() {
    bindButtons();
    bindUploads();
    onGoogleReady.push(getAllTestsAdmin);
    console.info("Initialized admin script!");
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", async (e) => {
        e.preventDefault();
        await submit();
    }, false);
    document.getElementById("delete").addEventListener("click", async (e) => {
        e.preventDefault();
        await remove();
    }, false);
    document.getElementById("included-add").addEventListener("click", (e) => {
        e.preventDefault();
        addIncluded();
    }, false);
    document.getElementById("tester-add").addEventListener("click", (e) => {
        e.preventDefault();
        addTester();
    }, false);
    document.getElementById("required-add").addEventListener("click", (e) => {
        e.preventDefault();
        addRequired();
    }, false);
    document.getElementById("stipulatable-add").addEventListener("click", (e) => {
        e.preventDefault();
        addBlankStipulatable();
    })
    document.querySelector("input[autocorrect]").addEventListener("change", (e) => {
        e.preventDefault();
        modified = true;
    })
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
    modified = false;
    document.getElementById("manager").classList.add("hidden");
    document.getElementById("included").innerHTML = "";
    document.getElementById("testers").innerHTML = "";
    document.getElementById("required").innerHTML = "";
}

function generateFileExplorer(formatted) {
    let fileManager = document.getElementById("manager");
    fileManager.querySelector("input").value = formatted.name;
    fileManager.querySelector("textarea").value = formatted.description;
    generateInputs(formatted.includedFiles, document.getElementById("included"));
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
                statusChar.innerHTML = "&#10008;";
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
                    for (let label of inputArea.children) {
                        if (label === e.target || label.contains(e.target)) {
                            let oldOrder = label.style.order;
                            label.style.order = startSwap.style.order;
                            startSwap.style.order = oldOrder;
                            modified = true;
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
            modified = true;
        });
        labelName.addEventListener("change", (e) => {
            e.preventDefault();
            modified = true;
        })
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
            testBtn.innerHTML += " &#10008;";
        }
        else {
            testBtn.classList.add("normal");
            testBtn.innerHTML += " &#10004;";
        }
        testBtn.addEventListener("click", async (e) => {
            await getTestAdmin(e.target.name);
        })
        list.appendChild(testBtn);
    }
}

async function getAllTestsAdmin() {
    clearInputs();
    let error = null;
    try {
        let response = await fetch("/Admin/Tester/GetAll", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken()
            })
        })
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            let data = await response.json();
            generateList(data);
            bindUploads();
        }
    } catch (e) {
        error = e;
    }
    if (error != null) {
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    }
}

function warnModified(callback) {
    triggerPopup("Stipulatable has been modified!", "Are you sure you want to discard changes?")
    let discardBtn = document.createElement("button");
    discardBtn.className = "danger form-control";
    discardBtn.innerHTML = "Discard";
    discardBtn.addEventListener("click", (e) => {
        e.preventDefault();
        modified = false;
        callback();
    }, false);
    triggerPopupButtons([discardBtn]);
}

async function getTestAdmin(num) {
    if (modified) {
        warnModified(() => getTestAdmin(num));
        return;
    }
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    try {
        let response = await fetch("/Admin/Tester/Get", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "TestID": id
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            let data = await response.json();
            if (!data.hasOwnProperty("testerFiles")) {
                console.error("Could not find the tester files from tester!");
                return;
            }
            if (!data.hasOwnProperty("requiredFiles")) {
                console.error("Could not find the required files from tester!");
                return;
            }

            generateFileExplorer(data);
            bindUploads();
            currentTest = id;
        }
    }
    catch (e) {
        handleErrors(0, e);
    }
}

async function submit() {
    let message = ["Now updating stipulatable information..."];
    triggerPopup("Updating...", message.join('\n'));
    document.getElementById("submit").disabled = true;

    try {
        message.push("---\n");
        if (currentTest > 0)
            await updateStipulatable(message);
        else
            await addStipulatable(message);
        message.push("---\nNow uploading source files, if any...");
        triggerPopup("Updating...", message.join('\n'));
        await uploadSourceFiles(message);
        message.push("---\nNow uploading included files, if any...");
        triggerPopup("Updating...", message.join('\n'));
        await uploadIncludeFiles(message);
        triggerPopup("Finished updating!", message.join('\n'));
        await getAllTestsAdmin();
        await getTestAdmin(currentTest);
    } catch (e) {
        document.getElementById("submit").disabled = false;
        message.push(e);
        handleErrors(0, message.join('\n'))
    }
}

async function uploadSourceFiles(message) {
    let fileInputs = document.querySelectorAll("#testers input[type='file']");
    await upload(message, fileInputs, "/Admin/Tester/UploadInclude");
}

async function uploadIncludeFiles(message) {
    let fileInputs = document.querySelectorAll("#included input[type='file']");
    await upload(message, fileInputs, "/Admin/Tester/UploadInclude");
}

async function upload(message, fileInputs, uploadTo) {
    let data = new FormData();

    data.append('AuthToken', getLoginToken());
    data.append('ConnectionID', "no");
    if (currentTest == null)
        currentTest = 0;
    data.append('TestID', JSON.stringify(currentTest));
    for (let input of fileInputs) {
        if (input.files.length === 0)
            continue;
        data.append('FileNames', input.parentElement.querySelector("input:not([type])").value);
        data.append('Files', input.files[0]);
        data.append('IsTester', JSON.stringify(false));
    }

    let response = await fetch(uploadTo, {
        method: "POST",
        body: data
    });
    if (response.status >= 400 && response.status < 600)
        handleErrors(response.status, null)
    else {
        let data = await response.text();
        let text = data.toString();
        message.push(text);
    }
}

async function updateStipulatable(message) {
    try {
        let response = await fetch("/Admin/Tester/Update", {
            method: "PUT",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "TestID": currentTest,
                "TestName": document.querySelector("input[autocorrect]").value,
                "TestDescription": document.querySelector("textarea").value,
                // William, what the heck? (Gets all children, extracts name from each input element)
                "Required": [...document.getElementById("required").children].map(o => o.getElementsByTagName("input")[0].value),
                // ... (Get all children under the testers, sort by flexbox order for draggables, then grab the names of each tester)
                "Testers": [...document.getElementById("testers").children].sort((p, q) => parseInt(p.style.order) - parseInt(q.style.order)).map(o => o.getElementsByTagName("input")[1].value),
                "Included": [...document.getElementById("included").children].sort((p, q) => parseInt(p.style.order) - parseInt(q.style.order)).map(o => o.getElementsByTagName("input")[1].value)
            })
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null)
        else {
            let data = await response.json();
            if (!data)
                message.push("Server failed to find a record; was it deleted?");
            else
                message.push("Successfully updated the stipulatable's information!\nDone!");
        }
    }
    catch (e) {
        document.getElementById("submit").disabled = false;
        message.push(e);
        triggerPopup("Error updating!", message.join('\n'));
    }
}

async function addStipulatable(message) {
    try {
        let response = await fetch("/Admin/Tester/Add", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "AuthToken": getLoginToken(),
                "TestName": document.querySelector("input[autocorrect]").value,
                "TestDescription": document.querySelector("textarea").value,
                "Required": [...document.getElementById("required").children].map(o => o.getElementsByTagName("input")[0].value),
                "Testers": [...document.getElementById("testers").children].sort((p, q) => parseInt(p.style.order) - parseInt(q.style.order)).map(o => o.getElementsByTagName("input")[1].value),
                "Included": [...document.getElementById("included").children].sort((p, q) => parseInt(p.style.order) - parseInt(q.style.order)).map(o => o.getElementsByTagName("input")[1].value)
            })
        })
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null)
        else {
            let data = await response.json();
            if (data <= 0)
                message.push("Server failed to return a valid ID...");
            else
                message.push("Successfully added the stipulatable (id " + data + ")!\nDone!");
            currentTest = data;
            triggerPopup("Finished updating!", message.join('\n'));
        }
    }
    catch (e) {
        document.getElementById("submit").disabled = false;
        message.push(e);
        handleErrors(0, message.join('\n'));
    }
}

async function remove() {
    let name = document.querySelector("input[autocorrect]").value;
    let deleteBtn = document.createElement("button");
    deleteBtn.className = "danger form-control";
    deleteBtn.innerHTML = "Delete";
    deleteBtn.addEventListener("click", async (e) => {
        e.preventDefault();
        await actuallyDelete();
    }, false);
    triggerPopup("Delete?", "Are you sure you want to delete the test for \"" + name + "\" (id " + currentTest + ")?");
    triggerPopupButtons([deleteBtn]);
}

async function actuallyDelete() {
    clearInputs();
    if (!(currentTest > 0)) // Why not <= 0? Because null and undefined.
        return;
    try {
        let response = await fetch("/Admin/Tester/Delete", {
            method: "DELETE",
            headers: {
                'Content-Type': 'application/json'
            },
            body: currentTest
        });
        if (response.status >= 400 && response.status < 600)
            handleErrors(response.status, null);
        else {
            let data = await response.json();
            if (!data)
                triggerPopup("Mukyu~", "The server couldn't delete anything! Is the cache old?");
            await getAllTestsAdmin();
        }
    }
    catch (e) {
        
    }
}

function addIncluded() {
    let inputArea = document.getElementById("included");
    addDragButton(inputArea, false);
}

function addTester() {
    let inputArea = document.getElementById("testers");
    addDragButton(inputArea, true);
}

function addRequired() {
    modified = true;
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
        modified = true;
    });
    labelName.addEventListener("change", (e) => {
        e.preventDefault();
        modified = true;
    })
    label.appendChild(removeButton);
    inputArea.appendChild(label);
}

function addDragButton(inputArea, warn) {
    modified = true;

    let order = [...inputArea.children].sort((p, q) =>  parseInt(q.style.order) - parseInt(p.style.order));
    order = order.length === 0 ? 0 : parseInt(order[0].style.order) + 1; // If there are no items, the first order is zero, otherwise it's the greatest order (by sort), then +1.

    let label = document.createElement("label");
    let labelName = document.createElement("input");
    label.className = "input";
    let removeButton = document.createElement("button");
    let input = document.createElement("input");
    input.type = "file";
    input.addEventListener("change", (e) => {
        if (warn && [...document.getElementById("required").children]
            .map(o => o.getElementsByTagName("input")[0].value)
            .find(o => o === e.target.files[0].name) !== undefined) {
            triggerPopup("Mukyu~", "You cannot attach a file for something required from the user!");
            e.target.parentNode.className = "input danger";
            e.target.value = null;
            return;
        }
        if (e.target.value === "")
            e.target.parentNode.className = "input normal";
        else {
            e.target.parentNode.className = "input success";
            labelName.value = e.target.files[0].name;
        }

    }, false);

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
            for (let label of inputArea.children) {
                if (label === e.target || label.contains(e.target)) {
                    let oldOrder = label.style.order;
                    label.style.order = startSwap.style.order;
                    startSwap.style.order = oldOrder;
                    modified = true;
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
        modified = true;
    });
    labelName.addEventListener("change", (e) => {
        e.preventDefault();
        modified = true;
    })
    label.appendChild(removeButton);
    inputArea.appendChild(label);
}

function addBlankStipulatable() {
    if(modified) {
        warnModified(() => addBlankStipulatable());
        return;
    }
    currentTest = null;
    clearInputs();
    document.querySelector("input[autocorrect]").value = "";
    document.getElementById("manager").classList.remove("hidden");
    document.getElementById("submit").disabled = false;
    document.getElementById("delete").disabled = false;
}