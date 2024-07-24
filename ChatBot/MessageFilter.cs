using ChatBot.Authentication;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Linq;

namespace ChatBot
{
    public static class MessageFilter
    {
        private static List<string> suspiciousStrings = new List<string> { "bigfollows", "bigfollows", "primes and", "buy",
            "qualitу", "service", "custom graphics", "sorry", "interrupting", "channel",
            "portfolio", "follow", "view", "bot", "price", "quality", "convenient", "cheap", "offer", 
            "free", "code", "guarantee", "satisfaction", "best" };

        public static bool IsMessageSpam(string message, bool isFirstMessage)
        {
            if (isFirstMessage && IsURLInsideMessage(message))
                return true;

            // Assume anything starting with an ! is a command, which isn't spam
            if (message.Trim().StartsWith("!"))
                return false;

            int spamCutoff = 4;
            int spamWarning = 0;

            foreach (string s in suspiciousStrings)
            {
                if (message.ToLower().Contains(s))
                    spamWarning++;
            }

            return (spamWarning >= spamCutoff) ?  true : false;
        }

        public static bool IsStringURL(string word)
        {
            if (word.Length < 4)
                return false;

            // assume messages in the form of aa.xyz or aa.zy are urls
            if (word.Substring(word.Length - 4, 1) == "." || word.Substring(word.Length - 3, 1) == ".")
                return true;

            return false;
        }

        public static bool IsURLInsideMessage(string message)
        {
            foreach (string word in message.Split(" "))
            {
                if (IsStringURL(word))
                    return true;
            }
            return false;
        }

        public static async void TimeoutUser(string userID)
        {
            try
            {
                // Setup necessary boilerplate to time a user out
                var API = TwitchAPIFactory.GetAPI();
                string broadcastorID;
                string moderatorID;

                moderatorID = GetChannelID(Config.botUsername);
                broadcastorID = GetChannelID(Config.channelUsername);

                BanUserRequest banRequest = new BanUserRequest();
                banRequest.UserId = userID;
                banRequest.Duration = 5; // todo change this into a well thought out number (300?)
                banRequest.Reason = "Anti-bot spam filter triggered";

                // Execute the timeout
                var result = await API.Helix.Moderation.BanUserAsync(broadcastorID,
                    moderatorID,
                    banRequest,
                    await OAuth.GetAccessToken(OAuth.eScope.ban));
            }
            catch (Exception ex)
            {
                SystemLogger.Log($"Failed to timeout message in MessageReceiver.cs\n\n{ex}");
            }
        }


        public static string GetChannelID(string channelName)
        {
            return TwitchAPIFactory.GetAPI().Helix.Users.GetUsersAsync(logins: new List<string> { channelName })
                .Result.Users.First().Id;
        }
    }
}

