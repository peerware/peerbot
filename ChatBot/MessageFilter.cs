using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace ChatBot
{
    public static class MessageFilter
    {
        public static bool isMessageSpam(string message)
        {
            List<string> suspiciousStrings = new List<string> { "bigfollows", "bigfollows", "primes and", "buy", "qualitу", "service", "tор", "streamer",
             "custom graphics", "sorry", "interrupting", "channel", "portfolio", "you", "follow", "view", "bot", "com", "org",
             "price", "quality", "convenient"};

            int spamCutoff = 2;
            int spamWarning = 0;

            foreach (string s in suspiciousStrings)
            {
                if (message.ToLower().Contains(s))
                    spamWarning++;
            }

            if (spamWarning >= spamCutoff)
                return true;
            else
                return false;
        }

        public static bool IsMessageWebsiteURL(string message)
        {
            return message.StartsWith("!") || message.Contains("http") || message.Contains("org") || message.Contains("com") || message.Contains("www.") ? false : true;
        }
    }
}

