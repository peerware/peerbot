using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using System.Threading.Tasks;
using ChatBot.MessageSpeaker;
using TwitchLib.Client.Events;

namespace ChatBot
{
    public class MessageExecutor
    {
        TwitchAPI api = null;
        Streams streamObject = null;
        TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream channelStream = null;
        TwitchClient client = null;
        HivescorePoller hivescorePoller;
        DateTime InitializationTime;

        // Don't allow instantiation from outside this class (forces factory-ish instantiaiton)
        private MessageExecutor() { } 

        public static async Task<MessageExecutor> GetMessageExecutor(TwitchClient client)
        {
            // Boilerplate code
            MessageExecutor messageExecutor = new MessageExecutor();

            messageExecutor.api = TwitchAPIFactory.GetAPI();
            messageExecutor.streamObject = messageExecutor.api.Helix.Streams;
            messageExecutor.channelStream = (await messageExecutor.streamObject.GetStreamsAsync(null, null, 1, null, null, "all", null, new List<string> { Config.channelUsername })).Streams.FirstOrDefault();
            messageExecutor.client = client;

            // Poll for hivescore
            messageExecutor.hivescorePoller = new HivescorePoller(client);
            messageExecutor.hivescorePoller.BeginPolling();

            // Get the latest message in the logs (assume this is when the last stream happened, this might change in the future but its fine for now)
            messageExecutor.InitializationTime = MessageLogger.GetTimeOfLatestMessage();

            return messageExecutor;
        }

        public async void ExecuteMessage(ChatMessage message)
        {
            // Try refreshing the channel stream every message received in case the bot was launched before going live
            if (channelStream == null)
            {
                channelStream = (await streamObject.GetStreamsAsync(null, null, 1, null, null, "all", null, new List<string> { Config.channelUsername })).Streams.FirstOrDefault();
            }

            switch (GetMessageCommand(message.Message.ToLower().Trim()))
            {
                case "!commands":
                    ExecuteCommands();
                    break;
                case "!uptime":
                    ExecuteUptime();
                    break;
                case "!giveaway":
                    ExecuteGiveaway(message.Username);
                    break;
                case "!enter":
                    ExecuteEnter(message.Username);
                    break;
                case "!claim":
                    ExecuteClaim(message.Username);
                    break;
                case "!todo":
                    ExecuteTodo(message.Username);
                    break;
                case "!downtime":
                    ExecuteDowntime();
                    break;
                case "!av":
                    ExecuteAV();
                    break;
                case "!asmr":
                    ExecuteASMR();
                    break;
                case "!dpi":
                case "!sens":
                    ExecuteDPI();
                    break;
                case "!crosshair":
                case "!xhair":
                    ExecuteCrosshair();
                    break;
                case "!bot":
                    ExecuteBot();
                    break;
                case "!elo":
                case "!rank":
                    ExecuteRank();
                    break;
                case "!today":
                case "!daily":
                case "!stats":
                    ExecuteDailyStats();
                    break;
                case "!vm":
                    ExecuteViewModel();
                    break;
                case "!tts":
                    ExecuteSaveTTSSettings(message.Username, GetMessageArgument(message.Message.ToLower().Trim()));
                    break;
                case "!followage":
                    ExecuteFollowage(message);
                    break;
                case "!yesterday":
                    ExecuteYesterday();
                    break;
                case "!emotes":
                    ExecuteEmotes();
                    break;
            }
        }

        private string GetMessageCommand(string message)
        {
            int IndexOfSpace = message.Trim().IndexOf(" ");

            if (IndexOfSpace > 0)
                return message.Substring(0, message.IndexOf(" ")).ToLower();
            else
                return message;
        }

        private string GetMessageArgument(string message)
        {
            int IndexOfSpace = message.Trim().IndexOf(" ") + 1;

            if (IndexOfSpace > 0)
                return message.Substring(IndexOfSpace).ToLower();
            else
                return "";
        }

        private void ExecuteCommands()
        {
            Say("!uptime !giveaway !downtime !av !sens !xhair !tts !followage !yesterday !elo !emotes");
        }

        private void ExecuteYesterday()
        {
            int hivescoreDifference = HivescoreLogger.GetHivescoreChange(DateTime.Today.AddDays(-1));

            if (hivescoreDifference == 0)
                Say("no data Sadge");
            else if (hivescoreDifference > 0)
                Say("+" + hivescoreDifference + " since yesterday ");
            else if (hivescoreDifference < 0)
                Say(Math.Abs(hivescoreDifference) + " since yesterday ");
        }

        private void ExecuteGiveaway(string username)
        {
            Say("@" + username + " use !claim");
        }

        private void ExecuteClaim(string username)
        {
            Say("@" + username + " use !enter");
        }

        private void ExecuteEnter(string username)
        {
            Say("@" + username + " use !giveaway");
        }

        private void ExecuteTodo(string username)
        {
            Say("@" + username + " :pencil2:");
        }

        private async void ExecuteFollowage(ChatMessage message)
        {

            var GetUsersResponse = await api.Helix.Users.GetUsersFollowsAsync(fromId: message.UserId);

            try
            {
                var followAge = new DateTime(DateTime.Now.ToUniversalTime().Ticks - (GetUsersResponse.Follows.FirstOrDefault(o => o.FromUserName == message.Username && o.ToUserName == Config.channelUsername).FollowedAt.ToUniversalTime().Ticks));
                Say("following for " + followAge.Hour.ToString() + " hours " + followAge.Minute.ToString() + " minutes and " + followAge.Second.ToString() + " seconds");

            }
            catch (Exception e)
            {
                Say("not following ;^(");
            }
        }

        private void ExecuteASMR()
        {
            Say("asmrs so he still \"comms\"");
        }

        private void ExecuteEmotes()
        {
            Say("5Head AYAYA EZY FeelsBadMan FeelsDankMan Flushed Hmm KEKW LULW mericCat monkaS OMEGALUL PauseChamp peepoSad pepeGun PepeHands");
            Say("PepeLaugh PepoG Pog POGGERS Prayge Stuckge Sweating widepeepoHappy widepeepoSad pepeRun BoneZone Bedge SUSSY AlienPls Sadge");
            Say("VeryPog TrollDespair pepeMeltdown lickL pepeSadJam hoSway PensiveWobble pepeDS RareMonkey");
        }

        private void ExecuteBot()
        {
            Say("this handmade bot runs on .net core" );
        }

        private void ExecuteRank()
        {
            int hivescoreChange = HivescoreLogger.GetHivescoreChange(DateTime.Today.AddDays(-1));

            if (hivescoreChange >= 0)
                Say(HivescoreFetcher.FetchHivescore().Result + " hivescore +" + Math.Abs(hivescoreChange) + " since yesterday");
            else if (hivescoreChange < 0)
                Say(HivescoreFetcher.FetchHivescore().Result + " hivescore -" + Math.Abs(hivescoreChange) + " since yesterday");
        }

        private void ExecuteUptime()
        {
            if (channelStream != null)
            {
                DateTime uptime = new DateTime((DateTime.Now.ToUniversalTime() - channelStream.StartedAt.ToUniversalTime()).Ticks);

                Say("live for " + uptime.Hour.ToString() + " hours " + uptime.Minute.ToString() + " minutes and " + uptime.Second.ToString() + " seconds");
            }
            else
            {
                Say("not live Sadge");
            }
        }

        private void ExecuteDowntime()
        {
            DateTime downtime = new DateTime((DateTime.Now.ToUniversalTime() - InitializationTime.ToUniversalTime()).Ticks);

            Say("ResidentSleeper for " + downtime.Hour.ToString() + " hours " + downtime.Minute.ToString() + " minutes and " + downtime.Second.ToString() + " seconds");
        }

        private void ExecuteAV()
        {
                Say("https://steamcommunity.com/sharedfiles/filedetails/?id=2331671641&searchtext=SCHNIGHTSY");
        }

        private void ExecuteCrosshair()
        {
                Say("11 length 3 width yellow cross with 2 pixel black border");
        }

        private void ExecuteDPI()
        {
            Say("800 dpi 2.0 marine 3.96 alien");
        }

        private void TimeUserOut(string Username)
        {
            Say("/timeout " + Username + " 1");
        }

        private void ExecuteDailyStats()
        {
            Say(hivescorePoller.GetDailyStatsMessage());
        }

        private void ExecuteViewModel()
        {
            Say("misc -> viewmodel" );
        }

        private void ExecuteSaveTTSSettings(string Username, string Arguments)
        {
            // TTS arguments come in the form of <Type> <Value>
            string TTSCommand = GetMessageCommand(Arguments);
            string TTSArguments = GetMessageArgument(Arguments);

            // Special case for user friendly commands that dont have an argument
            if (Arguments.StartsWith("enable") ||Arguments.StartsWith("on"))
                TTSCommand = "enable";
            else if (Arguments.StartsWith("disable") || Arguments.StartsWith("off"))
                TTSCommand = "disable";

            // If theres no command assume they want help
            if (TTSCommand == "" || TTSCommand == "help")
                TTSCommand = "help";

            // Get existing settings - constructor will provide defaults otherwise
            MessageSpeakerSettings settings = MessageSpeakerSettingsManager.GetSettingsFromStorage(Username);
            settings.twitchUsername = Username;

            // Overwrite settings if we get a valid command
            switch (TTSCommand)
            {
                case "help":
                    Say("!tts <setting> <value>. Settings: on/off, speed (0.55-2), pitch (-20 to 20), man, woman, dialect (aus, ireland, south africa, uk, america, french, japanese, bog)");
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "enable":
                    settings.SetIsSpeechEnabled("enable");
                    Say("tts on");
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "disable":
                    settings.SetIsSpeechEnabled("disable");
                    Say("tts off");
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "speed":
                    double speed = double.TryParse(TTSArguments, out _) ? double.Parse(TTSArguments) : 0;

                    if (speed < 0.5 || speed > 2)
                        Say("@" + Username + " enter a number from 0.5-2");
                    else
                    {
                        settings.SetSpeed(speed);
                        Say("speed set");
                    }
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "pitch":
                    double pitch = double.TryParse(TTSArguments, out _) ? double.Parse(TTSArguments) : 0;

                    if (pitch < -20 || pitch > 20)
                        Say("@" + Username + " enter a number from -20 to 20");
                    else
                    {
                        settings.SetPitch(pitch);
                        Say("pitch set");
                    }
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "dialect":
                    {
                        bool IsSaved = settings.SetLanguage(TTSArguments);

                        if (IsSaved)
                            Say("dialect saved");
                        else
                            Say("@" + Username + " Choose between australian, irish, german, italian, south african, british, american, french, japanese");
                        MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                        break;
                    }
                case "frenchwoman":
                    {
                        MessageSpeakerSettingsManager.SetPresetVoice(Username, MessageSpeakerSettingsManager.voicePresets.frenchWoman);
                        Say("dialect saved");
                        break;
                    }
                case "bog":
                    {
                        MessageSpeakerSettingsManager.SetPresetVoice(Username, MessageSpeakerSettingsManager.voicePresets.bog);
                        Say("dialect saved");
                        break;
                    }
                case "frenchman":
                    {
                        MessageSpeakerSettingsManager.SetPresetVoice(Username, MessageSpeakerSettingsManager.voicePresets.frenchMan);
                        Say("dialect saved");
                        break;
                    }
                case "man":
                case "woman":
                case "male":
                case "female":
                case "unspecified":
                case "neutral":
                    SaveGenderSayResult(settings, TTSCommand, Username);
                    break;
                case "silence":
                    {
                        Config.IsTextToSpeechEnabled = false;
                        Say("tts silenced");
                        break;
                    }
                case "unsilence":
                    {
                        Config.IsTextToSpeechEnabled = true;
                        Say("tts unsilenced");
                        break;
                    }
            }
        }

        private void SaveGenderSayResult(MessageSpeakerSettings settings, string TTSArguments, string Username)
        {
            bool IsSaved = settings.SetGender(TTSArguments);

            if (IsSaved)
                Say("gender saved");
            else
                Say("@" + Username + " Choose between man, woman, neutral, and unspecified");
            MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
        }

        private void Say(string Message)
        {
            client.SendMessage(Config.channelUsername, Message);
        }
    }
}
