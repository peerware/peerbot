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
            bool isMessageBanned = IsMessageSpam(message) || IsMessageNegative(username, message);

            if (isMessageBanned)
                DeleteMessage(username, client);

            return isMessageBanned;
        }

        private static bool IsMessageSpam(string message)
        {
            message = message.ToLower();

            if (message.Contains("bigfollows") || message.Contains("t to become famous") || message.Contains("primes and") || message.Contains("buy followers") || message.Contains("racis"))
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
            client.SendMessage(Config.channelUsername, "/ban " + Username);
        }
    }
}
