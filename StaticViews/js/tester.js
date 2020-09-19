'use strict';

onInitTester();

function onInitTester() {
    bindButtons();
    bindUploads();
    console.log("Initialized tester script!");
}

function bindButtons() {
    document.getElementById("submit").addEventListener("click", (e) => {
        e.preventDefault();
        submit();
    }, false);

    document.getElementById("testId").addEventListener("change", (e) => {
        getTest(e.target.value);
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
}

function submit() {
    let data = new FormData();
    
    document.querySelectorAll("input[type='file']").forEach((input) => {
            data.append('FileNames', input.name);
            for (let i = 0; i < input.files.length; i++)
                data.append('Files', input.files[i]);
            data.append('IsTester', JSON.stringify(false));
            data.append('TestID', JSON.stringify(parseInt(document.getElementById("testId").value, 10)))
            data.append('AuthToken', getLoginToken())
        }
    );

    document.getElementById("result").innerHTML = "Now loading...";
    document.getElementById("submit").disabled = true;

    fetch("/Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => {
        if (response.status >= 400 && response.status < 600)
            throw new Error("HTTPERR " + response.status);
        return response.text();
    })
    .then((formatted) => {
        document.getElementById("result").innerHTML = formatted;
    })
    .catch((error) => {
        if (error.message.startsWith("HTTPERR")) {
            switch (parseInt(error.message.substr(7))) {
                case 403:
                    error = "Login credentials failed, try logging out and logging back in!";
                    break;
                case 404:
                    error = "Could not locate the uploader... try refreshing the page?";
                    break;
                case 500:
                    error = "The server decided that it wanted to die. Ask William about what the heck you did to kill it.";
                    break;
            }
        }
        
        console.error("We had an error... ", error);
        triggerPopup("Mukyu~", error);
    })
    .finally(() => {
        document.getElementById("submit").disabled = false;
    });
}

function getTest(num) {
    let id = parseInt(num, 10);
    if (isNaN(id))
        return;
    clearInputs();
    fetch("/Tester/GetTest", {
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
        return response.json()
    })
    .then((formatted) => {
        if(!formatted.hasOwnProperty("requiredFiles")) {
            console.error("Could not find the required files from tester!");
            return;
        }
        generateInputs(formatted.requiredFiles);
        bindUploads();
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

function clearInputs() {
    let inputArea = document.getElementById("fileInputs");
    inputArea.innerHTML = "";
    document.getElementById("submit").disabled = true;
}

function generateInputs(names) {
    let inputArea = document.getElementById("fileInputs");
    for (let file of names) {
        let label = document.createElement("label");
        label.className = "input normal";
        let input = document.createElement("input");
        input.type = "file";
        input.name = file;
        label.innerHTML = file;
        label.appendChild(input);
        inputArea.appendChild(label);
    }
    document.getElementById("submit").disabled = false;
}