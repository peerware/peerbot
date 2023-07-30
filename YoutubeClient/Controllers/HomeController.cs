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

namespace YoutubeClient.Controllers
{
    public class HomeController : Controller
    {

        private string links = null;

        private readonly ILogger<HomeController> _logger;

        private MessageSpeaker messageSpeaker = new MessageSpeaker();
        private MessageReceiver messageReceiver = new MessageReceiver();

        public HomeController(ILogger<HomeController> logger)
        {
            messageReceiver.twitchClient.OnMessageReceived += Client_OnMessageReceived;
            _logger = logger;
        }


        /// <summary>
        /// Plays audio in the browser when recieving a message
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMessageAudio()
        {
            MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
                .GetVoiceAudio("peerlessunderthestars", "test", messageSpeaker.ttsClient).ToArray());

            return File(audioMemoryStream, "audio/wav");
        }


        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            GetMessageAudio();
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
    }
}
