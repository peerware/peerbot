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

namespace YoutubeClient.Controllers
{
    public class HomeController : Controller
    {

        private string links = null;

        private readonly ILogger<HomeController> _logger;

        private MessageSpeaker messageSpeaker = new MessageSpeaker();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult GetMessageAudio()
        {
            MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
                .GetVoiceAudio("peerlessunderthestars", "test", messageSpeaker.client).ToArray());

            return File(audioMemoryStream, "audio/wav");
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
