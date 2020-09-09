'use strict';

onInit();

function onInit() {
    document.getElementById("submit").addEventListener("click", (e) => {
        e.preventDefault();
        submit();
    }, false);
}

function submit() {
    let data = new FormData();
    const program = document.querySelectorAll('input[type="file"]');
    for (let a = 0; a < program.length; a++) {
        let input = program[a];
        data.append('FileNames', input.name);
        for (let i = 0; i < input.files.length; i++) {
            data.append('Files', input.files[i]);
        }
        // bruh
        data.append('IsTester', JSON.stringify(false));
    }

    fetch("Tester/Upload", {
        method: "POST",
        body: data
    })
    .then((response) => response.text())
    .then((formatted) => {
        document.getElementById("result").innerHTML = formatted;
    });
}