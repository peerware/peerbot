using ChatBot.SteamWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            // todo MessageReceiver needs to be refactored so that its not the source of the TwitchClient
            // Can rename it or find a better way to integrate everything dependent on it
            // Monitor thunderdome lobbies. Info will go to the chat
            using (var tdwatcher = new LobbyWatcher(ReceivedLobbies,
                l => l.AverageElo >= 2000 && l.NumPlayers >= 8 && l.NumPlayers < l.MaxMembers && l.Status == ELobbyState.Queueing))
            {
                tdwatcher.BeginPolling();
            }
        }

        private void ReceivedLobbies(IEnumerable<ThunderDomeLobby> lobbies)
        {
            int count = lobbies.Count();
            for (int n = 0; n < count; ++n)
            {
                var l = lobbies.ElementAt(n);
                client.SendMessage(Config.channelUsername, 
                    $" Lobby {n}/{count} ({l.Id % 1000}): {l.NumPlayers}/{l.MaxMembers}, {l.AverageElo} skill");
            }
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
            if (e.ChatMessage.DisplayName == Config.botUsername)
                return;

            MessageLogger.LogMessage(e.ChatMessage);

            if (MessageFilter.FilterSpam(e.ChatMessage.Username, e.ChatMessage.Message, client))
                return;

            if (Config.IsTextToSpeechEnabled)
                messageSpeaker.SpeakMessage(e.ChatMessage);
             
            if (e.ChatMessage.Message.StartsWith("!"))
                messageExecutor.ExecuteMessage(e.ChatMessage);
        }
    }
}
