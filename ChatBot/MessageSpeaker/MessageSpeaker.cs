using TwitchLib.Client.Models;
using Google.Cloud.TextToSpeech.V1;
using System;
using System.Media;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ChatBot.MessageSpeaker
{
    public class MessageSpeaker
    {
        /// <summary>
        /// Returns false if the message shouldnt be spoken
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsMessageAllowedSpoken(string message)
        {
            return message.StartsWith("!") || message.StartsWith("http") || message.StartsWith("www.") ? false : true;
        }
    }
}
