﻿"use strict";

import { triggerPopup, disablePopup } from "/js/main.js";

export {connection, connectionId, onSocketReady}

let connection;
let connectionId;
let onSocketReady = [];

// noinspection JSIgnoredPromiseFromCall
onInitWebsocket();

async function onInitWebsocket() {
    console.log("Waiting for signalR...");
    waitForSignalR();
}

async function waitForSignalR(){
    if(typeof signalR !== "undefined")
        await connect();
    else
        setTimeout(waitForSignalR, 250);
}

async function connect() {
    console.log("Now attempting to connect to the Koumakan, please wait warmly...")
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/koumakan")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
    try {
        connection.onclose(e => {
            triggerPopup("WebSocket failed!", "Lost connection to the Koumakan...\nWe've already tried reconnecting, so you'll need to refresh the page if you need live updates.\nError: " + (e === undefined ? "Unknown..." : e));
        });
        await connection.start();
        connectionId = connection.connectionId;
        console.log(`Connected to the Koumakan! Your connection ID is ${connectionId}. Have a nice day.`);
        for (let f of onSocketReady)
            f(connection);
    } catch (e) {
        console.error("Error connecting to the Koumakan! " + e.toString());
    }
}

/*
connection.invoke("SendMessage", user, message).catch(function (err) {
    return console.error(err.toString());
});
 */