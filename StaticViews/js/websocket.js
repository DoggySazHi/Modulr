"use strict";

let connection;
let connectionId;

onInitWebsocket();

function onInitWebsocket() {
    console.log("Now attempting to connect to the Koumakan, please wait warmly...")
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/koumakan")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
    connection.start().then(() => {
        connectionId = connection.connectionId;
        console.log(`Connected to the Koumakan! Your connection ID is ${connectionId}. Have a nice day.`);

    }).catch((e) => {
        console.error("Error connecting to the Koumakan! " + e.toString());
    });
}

/*
connection.invoke("SendMessage", user, message).catch(function (err) {
    return console.error(err.toString());
});
 */