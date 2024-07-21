using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace ChatBot
{
    public static class MessageFilter
    {
        private static List<string> suspiciousStrings = new List<string> { "bigfollows", "bigfollows", "primes and", "buy",
            "qualitу", "service", "tор", "streamer", "com", "custom graphics", "sorry", "interrupting", "channel",
            "portfolio", "you", "follow", "view", "bot", "org", "price", "quality", "convenient", "cheap", "offer", 
            "free", "code", "guarantee", "satisfaction", "best" };

        public static bool IsMessageSpam(string message)
        {
            // Assume anything starting with an ! is a command, which isn't spam
            if (message.Trim().StartsWith("!"))
                return false;

            int spamCutoff = 3;
            int spamWarning = 0;

            foreach (string s in suspiciousStrings)
            {
                if (message.ToLower().Contains(s))
                    spamWarning++;
            }

            return (spamWarning >= spamCutoff) ?  true : false;
        }

        public static bool IsMessageWebsiteURL(string message)
        {
            return message.Contains("http") || message.Contains("org") || message.Contains("com") || message.Contains("www")
                ? true : false;
        }
    }
}

