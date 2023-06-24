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
        TwitchClient client;
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
            client = new TwitchClient(customClient);
            client.Initialize(credentials, Config.channelUsername);

            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();

            messageExecutor = MessageExecutor.GetMessageExecutor(client).Result;
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

            if (MessageFilter.FilterSpam(e.ChatMessage.Username, e.ChatMessage.Message, client))
                return;

            if (Config.IsTextToSpeechEnabled)
                messageSpeaker.SpeakMessage(e.ChatMessage.Username, e.ChatMessage.Message);
             
            if (e.ChatMessage.Message.StartsWith("!"))
                messageExecutor.ExecuteMessage(e.ChatMessage);
        }
    }
}
