"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/IrcHub").build();


connection.on("ReceiveBroadcastMessage",
    function(rawMessage) {
        outputMessage(rawMessage);
    });

document.getElementById("sendMessageButton").addEventListener("click",
    function(event) {
        var rawMessage = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", rawMessage).catch(function(err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

function outputMessage(message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
}

connection.start().catch(function (err) {
    return console.error(err.toString());
});