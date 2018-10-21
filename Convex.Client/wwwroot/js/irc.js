"use strict";

function outputMessage(message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
}

window.addEventListener("DOMContentLoaded",
    function() {
        var connection = new signalR.HubConnectionBuilder().withUrl("/IrcHub").build();
        connection.logging = true;
        connection.start().catch(function(err) {
            return console.error(err.toString());
        });

        connection.on("ReceiveBroadcastMessage",
            function(rawMessage) {
                outputMessage(rawMessage);
            });

        document.getElementById("sendMessageButton").addEventListener("click",
            function(event) {
                var rawMessage = document.getElementById("messageInput").value;
                connection.invoke("sendMessage", rawMessage).catch(function(err) {
                    return console.error(err.toString());
                });
                event.preventDefault();
            });
    });