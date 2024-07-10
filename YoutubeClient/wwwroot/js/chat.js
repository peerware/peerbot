"use strict";

var messageQueue = [];
var isMessagePlaying = false;


var songQueue = [];

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

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
    if (songQueue.length < 1)
        return;

    var videoPlayer = $('#songRequestPlayer');

    if ($('#isPlayerEnabled').val() == 'on') {

        videoPlayer.attr('src', songQueue[0]);
        songQueue.shift();

        setTimeout(function () {
            videoPlayer.click();
        }, 2000); // delay playing the video to ensure the video gets loaded
    }
}

connection.start();