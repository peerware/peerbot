using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YoutubeClient.Models;
using ChatBot.MessageSpeaker;
using System.IO;
using ChatBot;
using TwitchLib.Client.Events;
using YoutubeClient.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace YoutubeClient.Controllers
{
    public class HomeController : Controller
    {

        private string links = null;

        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> chatHub = null;

        private GoogleTTSSettings googleTTSSettings = new GoogleTTSSettings();
        private MessageReceiver messageReceiver = null;

        public HomeController(ILogger<HomeController> logger, IHubContext<ChatHub> chatHub)
        {
            if (this.chatHub != null)
                return;

            if (messageReceiver == null)
            {
                messageReceiver = new MessageReceiver();
                messageReceiver.twitchClient.OnMessageReceived += Client_OnMessageReceived;
            }

            _logger = logger;

            this.chatHub = chatHub;
        }


        /// <summary>
        /// Plays audio in the browser when recieving a message
        /// </summary>
        /// <returns></returns>
        public void GetMessageAudio(string username, string message)
        {
            MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
                .GetVoiceAudio(username, message, googleTTSSettings.ttsClient).ToArray());
            
            long audioLength = audioMemoryStream.Length;

            chatHub.Clients.All.SendAsync("ReceiveMessageAudio", System.Convert.ToBase64String(audioMemoryStream.ToArray()));

            return;
        }


        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            GetMessageAudio(e.ChatMessage.Username, e.ChatMessage.Message);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Commented out example of restful method that returns data, can be called from client
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //public ActionResult GetMessageAudio(string username, string message)
        //{
        //    MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
        //        .GetVoiceAudio(username, message, googleTTSSettings.ttsClient).ToArray());

        //    chatHub.Clients.All.SendAsync("ReceiveMessageAudio", System.Convert.ToBase64String(audioMemoryStream.ToArray()));
        //    // data is received by client, need to make memorystream audio play

        //    return null; //File(audioMemoryStream, "audio/wav");
        //}
    }
}
