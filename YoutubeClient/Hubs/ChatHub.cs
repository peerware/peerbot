using ChatBot;
using ChatBot.Messages.MessageSpeaker;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.Client.Events;
using YoutubeClient.Models;

namespace YoutubeClient.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessageAudio(string memoryStream)
        {
            await Clients.All.SendAsync("ReceiveMessageAudio", memoryStream);
        }

        public async Task SendSongRequest(VideoInfo videoInfo)
        {
            await Clients.All.SendAsync("ReceiveSongRequest", videoInfo);
        }
    }
}
