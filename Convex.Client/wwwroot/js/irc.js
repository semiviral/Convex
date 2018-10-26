"use strict";

window.addEventListener("DOMContentLoaded",
    function () {
        //#region MEMBERS

        var connection = new signalR.HubConnectionBuilder().withUrl("/IrcHub").build();
        var keyMap = {}

        //#endregion

        //#region CONNECTION

        connection.logging = true;
        connection.start().catch(function (err) {
            return console.error(err.toString());
        });

        connection.on("ReceiveBroadcastMessage",
            function (rawMessage) {
                appendMessage(rawMessage);
            });

        connection.on("ReceiveBroadcastMessageBatch", function (rawMessages) {
            rawMessages.forEach(message => { appendMessage(message) });
        });

        connection.on("ReceiveBroadcastMessageBatchPrepend", function (rawMessages) {
            rawMessages.forEach(message => { prependMessage(message) });
        });

        //#endregion

        //#region EVENT LISTENERS

        document.getElementById("messageInput").addEventListener("keydown", function (event) {
            mapKey(event.keyCode, event.keyType);

            keyMap[event.keyCode] = event.keyType == "onkeydown";

            console.log(event + " " + event);

            if (keyMap[16] == true) return;

            if (keyMap[13] == true)
                sendMessage(document.getElementById("messageInput").textContent);

            return false;
        });

        //#endregion

        //#region METHODS

        function sendMessage(rawMessage) {
            var rawMessage = document.getElementById("messageInput").value;

            connection.invoke("sendMessage", rawMessage).catch(function (err) {
                return console.error(err.toString());
            });

            event.preventDefault();
        }

        function prependMessage(message) {
            var li = document.createElement("li");
            li.textContent = cleanString(message);

            document.getElementById("messageList").insertBefore(li, document.getElementById("messageList").childNodes[0]);
        }

        function appendMessage(message) {
            var li = document.createElement("li");
            li.textContent = cleanString(message);

            document.getElementById("messageList").appendChild(li);
            document.getElementById("messageContainer").scrollTop = document.getElementById("messageContainer").scrollHeight;
        }

        function cleanString(message) {
            return message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

        }

        function mapKey(keyCode, keyType) {
            console.log("Map[" + keyCode + "] => " + keyType);
            keyMap[keyCode] = keyType == "keydown";
            console.log("Map[" + keyCode + "] => " + keyType);
        }

        //#endregion
    });