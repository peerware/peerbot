using System;
using System.Collections.Generic;
using System.Linq;
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
        MessageExecutor messageExecutor;
        MessageSpeaker.MessageSpeaker messageSpeaker;

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
            messageSpeaker = new MessageSpeaker.MessageSpeaker();
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
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            MessageLogger.LogMessage(e.ChatMessage);

            if (e.ChatMessage.DisplayName == Config.botUsername)
                return;

            if (MessageFilter.FilterSpam(e.ChatMessage.Username, e.ChatMessage.Message, twitchClient))
                return;
             
            if (e.ChatMessage.Message.StartsWith("!"))
                messageExecutor.ExecuteMessage(e.ChatMessage);
        }
    }
}
