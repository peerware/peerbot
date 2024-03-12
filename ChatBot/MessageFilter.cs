using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace ChatBot
{
    public static class MessageFilter
    {
        public static bool FilterSpam(string username, string message, TwitchClient client)
        {


            bool isMessageBanned = IsMessageSpam(username, message) || IsMessageNegative(username, message);

            if (isMessageBanned)
                DeleteMessage(username, client);

            // Don't allow URLs or commands to be spoken, but don't delete them either
            isMessageBanned = IsMessageWebsiteURL(message);

            return isMessageBanned;
        }

        private static bool IsMessageSpam(string username, string message)
        {
            List<string> suspiciousStrings = new List<string> { "bigfollows", "bigfollows", "primes and", "buy followers", "qualitу sеrvice", "tор streаmers",
             "custom graphics", "sorry", "interrupting", "channel", "portfolio", "you", "follow", "view", "bot",
             "price", "quality", "convenient"};

            int spamCutoff = 3;
            int probableSpam = 0;

            foreach (string s in suspiciousStrings)
            {
                if (message.ToLower().Contains(s))
                    probableSpam++;
            }

            if (probableSpam >= spamCutoff)
                return true;
            else
                return false;
        }

        private static bool IsMessageNegative(string username, string message)
        {
            return ((username.ToLower() == "ruggeddresscode" || username.ToLower().Contains("acmo3")) && message.ToLower().Contains("hypie"));
        }

        private static void DeleteMessage(string Username, TwitchClient client)
        {
            client.SendMessage(Config.channelUsername, "/timeout " + Username + " 1");
        }

        public static bool IsMessageWebsiteURL(string message)
        {
            return message.StartsWith("!") || message.StartsWith("http") || message.StartsWith("www.") ? false : true;
        }
    }
}

