using ChatBot;
using ChatBot.MessageSpeaker;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace YoutubeClient.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageAudio(string memoryStream)
        {
            await Clients.All.SendAsync("ReceiveMessageAudio", memoryStream);
        }

        public async Task SendSongRequest(string videoURL)
        {
            await Clients.All.SendAsync("ReceiveSongRequest", videoURL);
        }
    }
}
