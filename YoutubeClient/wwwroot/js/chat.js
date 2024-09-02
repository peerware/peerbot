"use strict";

var messageQueue = [];
var isMessagePlaying = false;


var songQueue = [];
let isSongPlaying = false;

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessageAudio", function (messageAudio) {
    if (messageAudio.length < 1)
        return; 

    messageQueue.push(messageAudio);
    ProcessMessageQueue();
});

// Plays all the messages in the queue
function ProcessMessageQueue() {
    if (isMessagePlaying || messageQueue.length < 1)
        return;

    try {
        let currentMessage = messageQueue[0];
        messageQueue.shift();

        // play all the messages in the queue while removing the played messages
        let audioPlayer = new Audio("data:audio/wav;base64," + currentMessage);
        audioPlayer.volume = $('#ttsVolume').val() / 100;

        audioPlayer.addEventListener("ended", function () {
            isMessagePlaying = false;
            ProcessMessageQueue();
        });

        isMessagePlaying = true;
        audioPlayer.play().catch(function (e) {
            isMessagePlaying = false;
        });
    }
    catch (error)
    {
        isMessagePlaying = false;
    }
}

connection.on("ReceiveSongRequest", function (videoInfo) {
    songQueue.unshift(videoInfo);
    TryPlaySong();
});

function TryPlaySong() {
    if (songQueue.length > 0 && !isSongPlaying)
        ProcessSongQueue();
    else if (songQueue.length > 0) {
        setTimeout(TryPlaySong, 5000);
    }
}

// Handles playing music
function ProcessSongQueue() {
    if (songQueue.length < 1 || isSongPlaying)
        return;

    var videoPlayer = $('#songRequestPlayer');

    if ($('#isPlayerEnabled').val() == 'on') {
        var songInfo = songQueue.shift();

        isSongPlaying = true;

        // Run this function again after the song plays to reset the isplaying flag
        setTimeout(TryPlaySongReset, songInfo.duration * 1000);

        videoPlayer.attr('src', songInfo.url);
        setTimeout(function () {
            videoPlayer.click();
        }, 2000); // delay playing the video to ensure the video gets loaded
    }
}

function TryPlaySongReset() {
    isSongPlaying = false;
    TryPlaySong();
}

connection.start();