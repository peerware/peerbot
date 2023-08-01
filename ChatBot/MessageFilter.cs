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
            message = message.ToLower();

            if (username == "moobot" || 
                message.Contains("bigfollows") || 
                message.Contains("t to become famous") || 
                message.Contains("primes and") || 
                message.Contains("buy followers")|| 
                message.Contains("qualitу sеrvice") || 
                message.Contains("rеal frее") || 
                message.Contains("tор streаmers") ||
                message.Contains("can i play with you? add me") ||
                message.Contains("custom graphics") ||
                message.Contains("sorry for interrupting") ||
                message.Contains("for your channel") ||
                message.Contains("you my portfolio") ||
                message.Contains("server crasher") ||
                message.Contains("if you need real") ||
                message.Contains("i found a cheat that won't get you banned") ||
                message.Contains("free and high quality") ||
                message.Contains("top streamer") ||
                message.Contains("your viewers") ||
                message.Contains("dot com") ||
                message.Contains("dogehype") ||
                (message.Contains("channel") && message.Contains("view") && message.Contains("follow")) ||
                message.Contains("your graphics/branding"))
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

