"use strict";

var messageQueue = [];
var isMessagePlaying = false;


var songQueue = [];
var isSongPlaying = false;

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} says ${message}`;
});

connection.on("ReceiveMessageAudio", function (messageAudio) {

    messageQueue.push(messageAudio);
    ProcessMessageQueue();
});

function ProcessMessageQueue() {
    if (isMessagePlaying || messageQueue.length < 1)
        return;

    // play all the messages in the queue while removing the played messages
    var audioPlayer = new Audio("data:audio/wav;base64," + messageQueue[0]);
    audioPlayer.volume = $('#ttsVolume').val() / 100;

    audioPlayer.addEventListener("ended", function () {
        isMessagePlaying = false;
        ProcessMessageQueue();
    });

    messageQueue.shift();
    isMessagePlaying = true;

    try { // If the user didn't interact with the page before the audio plays an exception will be thrown - try catch to prevent crash
        audioPlayer.play();
    }
    catch (error) { }
    
}

connection.on("ReceiveSongRequest", function (videoURL) {
    songQueue.push(videoURL);
    ProcessSongQueue();
});

// Handles playing music
function ProcessSongQueue() {
    if (isSongPlaying || SongQueue.length < 1)
        return;

    var videoPlayer = $('#songRequestPlayer');
    videoPlayer.attr('src', videoURL);

    if ($('#isPlayerEnabled').val() == 'on') {
        setTimeout(function () {
            videoPlayer.click();
        }, 2000); // delay playing the video to ensure the video gets loaded
    }
}