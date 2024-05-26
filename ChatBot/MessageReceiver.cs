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
        public TwitchClient twitchClient;
        MessageExecutor messageExecutor;

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
