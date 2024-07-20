using ChatBot.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace ChatBot
{
    public class MessageReceiver
    {
        public TwitchClient twitchClient;
        public MessageExecutor messageExecutor;

        // todo add channel name to this constructor
        public MessageReceiver()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(Config.botUsername, Config.botPassword);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            WebSocketClient customClient = new WebSocketClient(clientOptions);
            twitchClient = new TwitchClient(customClient);
            twitchClient.Initialize(credentials, Config.channelUsername);

            twitchClient.OnMessageReceived += Client_OnMessageReceived;

            twitchClient.Connect();

            messageExecutor = MessageExecutor.GetMessageExecutor(twitchClient).Result;
        }

        /// <summary>
        /// Logs the received message. 
        /// Does nothing if triggered by itself. 
        /// Filters Spam. 
        /// Executes Chat Commands.
        /// Optionally plays the received message with configuration options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            MessageLogger.LogMessage(e.ChatMessage);

            if (e.ChatMessage.DisplayName == Config.botUsername)
                return;

            if (MessageFilter.IsMessageSpam(e.ChatMessage.Message))
            {
                try
                {

                    // Setup necessary boilerplate to time a user out
                    var API = TwitchAPIFactory.GetAPI();
                    string broadcastorID;
                    string moderatorID;

                    moderatorID = API.Helix.Users.GetUsersAsync(logins: new List<string> { Config.botUsername })
                        .Result.Users.First().Id;
                    broadcastorID = API.Helix.Users.GetUsersAsync(logins: new List<string> { Config.channelUsername })
                        .Result.Users.First().Id;

                    BanUserRequest banRequest = new BanUserRequest();
                    banRequest.UserId = e.ChatMessage.UserId;
                    banRequest.Duration = 5; // todo change this into a well thought out number (300?)
                    banRequest.Reason = "spam filter triggered";

                    // Execute the timeout
                    var result = await API.Helix.Moderation.BanUserAsync(broadcastorID,
                        moderatorID,
                        banRequest,
                        await OAuth.GetBanAccessToken());
                }
                catch (Exception ex)
                {
                    SystemLogger.Log($"Failed to timeout message in MessageReceiver.cs\n\n{ex}");
                }

                return;
            }
             
            if (e.ChatMessage.Message.StartsWith("!"))
                messageExecutor.ExecuteMessage(e.ChatMessage);
        }
    }
}
