"use strict";

window.addEventListener("DOMContentLoaded", function () {
    //#region MEMBERS

    var connection = new signalR.HubConnectionBuilder().withUrl("/IrcHub").build();
    var keyMap = {};
    var selectedChannel = "";
    var channels = {}

    //#endregion

    //#region CONNECTION

    connection.logging = true;

    connection.on("ReceiveBroadcastMessage", function (message) {
        appendMessage(message);
    });

    connection.on("ReceiveBroadcastMessageBatch", function (message) {
        message.forEach(message => { appendMessage(message) });
    });

    connection.on("ReceiveBroadcastMessageBatchPrepend", function (message) {
        message.forEach(message => { prependMessage(message) });
    });

    connection.on("UpdateMessageInput", function (updatedInput) {
        document.getElementById("messageInput").value = updatedInput;
    });

    connection.on("BroadcastChannels", function (channels) {
        channels.forEach(channel => { addChannel(channel) });
    });

    connection.on("AddChannel", function (channel) {
        addChannel(channel);
    });

    connection.on("RemoveChannel", function (channelName) {

    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    //#endregion

    //#region EVENT LISTENERS

    document.getElementById("messageInput").addEventListener("keydown", function (event) {
        keyMap[event.keyCode] = true;

        switch (event.keyCode) {
            case 13:
                if (keyMap[16] === true) return;

                sendMessage(document.getElementById("messageInput").textContent);
                document.getElementById("messageInput").value = "";
                break;
        }

        if (keyMap[16] == false && keyMap[13] === true) {

        }
    });

    document.getElementById("messageInput").addEventListener("keyup", function (event) {
        keyMap[event.keyCode] = false;

        switch (event.keyCode) {
            case 38:
                updateMessageInput(true);
                break;
            case 40:
                updateMessageInput(false);
                break;
        }

    });

    //#endregion

    //#region CLIENT TO SERVER METHODS

    function getMessageBatchByChannel(channelName, startIndex, endIndex) {
        connection.invoke("requestBroadcastMessageBatch", channelName, false, startIndex, endIndex).catch(function (err) {
            return console.error(err.toString());
        });
    }

    function sendMessage(rawMessage) {
        var rawMessage = document.getElementById("messageInput").value;

        connection.invoke("sendMessage", rawMessage).catch(function (err) {
            return console.error(err.toString());
        });
    }

    function updateMessageInput(isForPreviousMessage) {
        connection.invoke("updateMessageInput", isForPreviousMessage).catch(function (err) {
            return console.error(err.toString());
        });
    }

    //#endregion 

    //#region GENERAL METHODS

    function addChannel(channel) {
        var newChannel = document.createElement("li");
        newChannel.innerHTML = channel["name"];

        console.log(channel);

        document.getElementById("channelList").appendChild(newChannel);
    }

    function prependMessage(message) {
        var li = document.createElement("li");
        li.textContent = message.Formatted;

        document.getElementById("messageList").insertBefore(li, document.getElementById("messageList").childNodes[0]);
    }

    function appendMessage(message) {
        console.log(message["origin"]);

        var li = document.createElement("li");
        li.textContent = message["formatted"];

        document.getElementById("messageList").appendChild(li);
        document.getElementById("messageContainer").scrollTop = document.getElementById("messageContainer").scrollHeight;
    }

    function encode(message) {
        return message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    }

    //#endregion
});