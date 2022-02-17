using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class MessageExecutor
    {
        TwitchAPI api = null;
        Streams streamObject = null;
        TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream channelStream = null;
        TwitchClient client = null;

        // Don't allow instantiation from outside this class (forces factory instantiaiton)
        private MessageExecutor() { } 

        public static async Task<MessageExecutor> GetMessageExecutor(TwitchClient client)
        {
            MessageExecutor messageExecutor = new MessageExecutor();

            messageExecutor.api = APIFactory.GetAPI();
            messageExecutor.streamObject = messageExecutor.api.Helix.Streams;
            messageExecutor.channelStream = (await messageExecutor.streamObject.GetStreamsAsync(null, null, 1, null, null, "all", null, new List<string> { Config.peerless_username })).Streams.FirstOrDefault();
            messageExecutor.client = client;

            return messageExecutor;
        }

        /// <summary>
        /// Takes a message
        /// </summary>
        public void ExecuteMessage(ChatMessage message)
        {
            switch (message.Message.ToLower().Trim())
            {
                case "!uptime":
                case "!downtime":
                    ExecuteUptime();
                    break;
                case "!av":
                    ExecuteAV();
                    break;
                case "!dpi":
                case "!sens":
                    ExecuteDPI();
                    break;
                case "!crosshair":
                case "!xhair":
                    ExecuteCrosshair();
                    break;
            }
        }

        // state the nature of your medical emergency
        // todo: implement !modlotto (grants a random user in the chat mod for XX time (how long? short, 5 min or less?)) 

        private void ExecuteUptime()
        {
            if (channelStream != null)
            {
                DateTime uptime = new DateTime((DateTime.Now.ToUniversalTime() - channelStream.StartedAt.ToUniversalTime()).Ticks);

                client.SendMessage(Config.peerless_username, "live for " + uptime.Hour.ToString() + " hours " + uptime.Minute.ToString() + " minutes and " + uptime.Second.ToString() + " seconds");
            }
        }

        private void ExecuteAV()
        {
                client.SendMessage(Config.peerless_username, "https://steamcommunity.com/sharedfiles/filedetails/?id=2331671641&searchtext=SCHNIGHTSY");
        }

        private void ExecuteCrosshair()
        {
                client.SendMessage(Config.peerless_username, "11 length 3 width yellow cross with 2 pixel black border");
        }

        private void ExecuteDPI()
        {
            client.SendMessage(Config.peerless_username, "800 dpi dont have a favorite sens yet");
        }
    }
}
