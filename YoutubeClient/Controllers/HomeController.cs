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
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Google.Cloud.TextToSpeech.V1;

namespace YoutubeClient.Controllers
{
    public class HomeController : Controller
    {

        private string links = null;

        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> chatHub = null;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.googleTTSVoices = Startup.GoogleTTSVoices.Select(item => new SelectListItem { Value = item.id.ToString(), Text = item.GetDisplayName() }).OrderBy(o => o.Text).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SetTTSVoice([FromBody] GoogleTTSVoice voice)
        {
            GoogleTTSVoice googleVoice = Startup.GoogleTTSVoices[voice.id];

            UserTTSSettings settings = new UserTTSSettings();
            settings.ttsSettings.SetSpeed(voice.speed);
            settings.ttsSettings.SetPitch(voice.pitch);
            settings.ttsSettings.SetGender(googleVoice.gender);
            settings.ttsSettings.languageCode = googleVoice.languageCode;
            settings.ttsSettings.voiceName = googleVoice.languageName;

            // If the username is "testvoice" append a string of random numbers to it
            if (voice.username.ToLower().Equals("testvoice"))
            {
                Random random = new Random();
                settings.twitchUsername = voice.username + random.Next(0, 4000);
            }
            else
                settings.twitchUsername = voice.username;

            UserTTSSettingsManager.SaveSettingsToStorage(settings);
            return Content(":)");
        }

        [HttpPost]
        public ContentResult TestTTSVoice([FromBody] GoogleTTSVoice voice)
        {
            //Update the static request counter and reject the request if we've exceeded the limit
            Interlocked.Increment(ref Startup.RequestCounter);
            if (Startup.RequestCounter > GoogleTTSSettings.MaximumRequests)
            {
                return Content(":(");
            }

            GoogleTTSVoice googleVoice = null;
            try
            {
                googleVoice = Startup.GoogleTTSVoices[voice.id];
            }
            catch (Exception ex)
            {
                Console.WriteLine("HomeController.TestTTSVoice failed to save a voice.");
                return Content(":(");
            }

            UserTTSSettings settings = new UserTTSSettings();
            settings.twitchUsername = "test";
            settings.ttsSettings.SetSpeed(voice.speed);
            settings.ttsSettings.SetPitch(voice.pitch);
            settings.ttsSettings.SetGender(googleVoice.gender);
            settings.ttsSettings.languageCode = googleVoice.languageCode;
            settings.ttsSettings.voiceName = googleVoice.languageName;
            UserTTSSettingsManager.SaveSettingsToStorage(settings);

            string inputString;
            if (voice.message.Length > 25)
                inputString = voice.message.Substring(0, 25);
            else
                inputString = voice.message;

            MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
            .GetVoiceAudio("test", inputString, Startup.ttsClient).ToArray());

            if (audioMemoryStream is null)
                return Content(":(");

            return Content(System.Convert.ToBase64String(audioMemoryStream.ToArray()));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
