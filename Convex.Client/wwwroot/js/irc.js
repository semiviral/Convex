"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/IrcHub").build();


connection.on("ReceiveBroadcastMessage",
    function(message) {
        outputMessage(message);
    });

connection.start().catch(function(err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click",
    function(event) {
        var message = document.getElementById("messageInput").value;
        connection.invoke("BroadcastMessage", message).catch(function(err) {
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
    document.getElementById("messagesList").scrollTop = document.getElementById("messagesList").scrollHeight;
}