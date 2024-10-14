using ChatBot.Authentication;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatBot.Messages
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

            int spamCutoff = isFirstMessage ? 1 : 5;
            int potentialSpamCount = 0;

            foreach (string s in suspiciousStrings)
            {
                if (message.ToLower().Contains(s))
                    potentialSpamCount++;
            }

            return potentialSpamCount >= spamCutoff ? true : false;
        }

        public static bool IsStringURL(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Regular expression pattern to identify URLs
            string pattern = @"[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?";

            // Create a Regex object
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            // Check if the input contains a URL
            return regex.IsMatch(input);
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

