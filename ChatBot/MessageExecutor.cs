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
using Newtonsoft;
using Newtonsoft.Json;

namespace ChatBot
{
    public class MessageExecutor
    {
        TwitchAPI api = null;
        Streams streamObject = null;
        TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream channelStream = null;
        TwitchClient client = null;
        HivescorePoller hivescorePoller;
        HivescorePoller tdPoller;
        DateTime InitializationTime;

        // Don't allow instantiation from outside this class (forces factory-ish instantiaiton)
        private MessageExecutor() { } 

        // todo make this initialize a bunch of stuff based on the provided channel name
        public static async Task<MessageExecutor> GetMessageExecutor(TwitchClient client)
        {
            // Boilerplate code
            MessageExecutor messageExecutor = new MessageExecutor();

            messageExecutor.api = TwitchAPIFactory.GetAPI();
            messageExecutor.streamObject = messageExecutor.api.Helix.Streams;
            messageExecutor.channelStream = (await messageExecutor.streamObject.GetStreamsAsync(null, null, 1, null, null, "all", null, new List<string> { Config.channelUsername })).Streams.FirstOrDefault();
            messageExecutor.client = client;

            // Poll for regular hivescore and td hivescore
            messageExecutor.hivescorePoller = new HivescorePoller(client, HivescorePoller.ePollingType.hivescore);
            messageExecutor.hivescorePoller.BeginPolling();

            messageExecutor.tdPoller = new HivescorePoller(client, HivescorePoller.ePollingType.td);
            messageExecutor.tdPoller.BeginPolling();

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

            // Make it so double exlamation mark commands entered accidentally work (!!tts speed 1)
            string cleanedMessage = message.Message;
            if (cleanedMessage.Length > 1 && cleanedMessage[0] == '!' && cleanedMessage[1] == '!')
                cleanedMessage = cleanedMessage.Substring(1, cleanedMessage.Length - 1);

            switch (GetMessageCommand(cleanedMessage.ToLower().Trim()))
            {
                case "!help":
                    ExecuteHelp();
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
                case "!idea":
                case "!suggestion":
                    ExecuteTodo(message.Username, message.Message);
                    break;
                case "!downtime":
                    ExecuteDowntime();
                    break;
                case "!av":
                    ExecuteAV();
                    break;
                case "!acmo":
                case "!ranger":
                case "!nightsy":
                case "!snowblind":
                case "!greenrex":
                case "!robp":
                case "!kittn":
                case "!golden":
                case "!schu":
                case "!xao":
                case "!zepar":
                    ExecuteFriendELO(message.Username, message.Message.Replace("!", ""));
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
                case "!team":
                    ExecuteTeam();
                    break;
                case "!specs":
                case "!pc":
                    ExecuteSpecs();
                    break;
                case "!fortnite":
                case "!fortnight":
                    ExecuteFortnite();
                    break;
                case "!elo":
                case "!rank":
                    ExecuteRank();
                    break;
                case "!today":
                case "!daily":
                    ExecuteDailyStats();
                    break;
                case "!tomorrow":
                    ExecuteTomorrow();
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
                case "!week":
                    ExecuteWeek();
                    break;
                case "!month":
                    ExecuteMonth();
                    break;
                case "!year":
                    ExecuteYear();
                    break;
                case "!stats":
                    ExecuteStats();
                    break;
                case "!quote":
                    ExecuteQuote(message.Username, message.Message);
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

        // !quote
        private void ExecuteQuote(string username, string message)
        {
            string arguments = GetMessageArgument(message);

            if (message == "!quote")
            {
                Say(QuoteManager.GetRandomQuote());
            }
            else if (arguments.StartsWith("add"))
            {
                int quoteNumber = QuoteManager.AddQuote(GetMessageArgument(arguments));
                Say("number " + quoteNumber + " added @" + username);
            }
            else if (Int32.TryParse(arguments, out _))
            {
                Say(QuoteManager.GetNumberedQuote(int.Parse(arguments)));
            }
        }

        // !help
        private void ExecuteHelp()
        {
            Say("!uptime !giveaway !downtime !av !quote !quote add <quote> !sens !xhair !bot !elo !today !week !month (broken) !tts !followage !yesterday !stats");
        }

        private void ExecuteYesterday()
        {
            Say(HivescoreFetcher.GetOldHivescoreMessage(DateTime.Today.AddDays(-1), HivescorePoller.ePollingType.hivescore));
        }

        // !giveaway
        private void ExecuteGiveaway(string username)
        {
            Say("@" + username + " use !claim");
        }

        // !week
        private void ExecuteWeek()
        {
            DateTime lastMonth = DateTime.Today.AddDays(-7);
            Say(HivescoreFetcher.GetOldHivescoreMessage(lastMonth, HivescorePoller.ePollingType.hivescore));
        }

        // !month
        private void ExecuteMonth()
        {
            DateTime lastMonth = DateTime.Today.AddDays(-31);
            Say(HivescoreFetcher.GetOldHivescoreMessage(lastMonth, HivescorePoller.ePollingType.hivescore));
        }

        // !year
        private void ExecuteYear()
        {
            DateTime lastMonth = DateTime.Today.AddDays(-365);
            Say(HivescoreFetcher.GetOldHivescoreMessage(lastMonth, HivescorePoller.ePollingType.hivescore));
        }

        // !stats
        private void ExecuteStats()
        {
            ExecuteYesterday();
            ExecuteWeek();
            ExecuteMonth();
        }
        
        // !claim
        private void ExecuteClaim(string username)
        {
            Say("@" + username + " use !enter");
        }

        // !enter
        private void ExecuteEnter(string username)
        {
            Say("@" + username + " use !giveaway");
        }

        // !todo, !idea, !suggestion
        private void ExecuteTodo(string username, string message)
        {
            if (message.Trim().ToLower() == "!todo")
                Say("There are " + IdeaLogger.GetNumberOfIdeas() + " ideas in the list right now");
            else
            {
                IdeaLogger.LogIdea(username, message);
                Say(IdeaLogger.GetFormattedSuccessMessage(username));
            }
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

        // !bot
        private void ExecuteBot()
        {
            Say("this handmade bot runs on .net 6" );
        }

        // !team 
        private void ExecuteTeam()
        {
            Say("euphie nano ritual tombrady me nabla");
        }

        // !team 
        private void ExecuteSpecs()
        {
            Say("5800x 3070 240hz G PRO ULTRALIGHT 60% keyboard red switches");
        }

        // !elo 
        private void ExecuteRank()
        {
            string hivescoreChange = HivescoreFetcher.GetHivescoreChange(DateTime.Today.AddDays(-1), HivescorePoller.ePollingType.hivescore);
            Say(HivescoreFetcher.FetchHivescore().Result + " hivescore (" + hivescoreChange + ") since yesterday");
        }

        // !uptime
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

        // !downtime
        private void ExecuteDowntime()
        {
            DateTime downtime = new DateTime((DateTime.Now.ToUniversalTime() - InitializationTime.ToUniversalTime()).Ticks);

            Say("ResidentSleeper for " + downtime.Hour.ToString() + " hours " + downtime.Minute.ToString() + " minutes and " + downtime.Second.ToString() + " seconds");
        }

        // !fortnite !fortnight
        private void ExecuteFortnite()
        {
            Say("no");
        }

        // !av
        private void ExecuteAV()
        {
            Say("https://steamcommunity.com/sharedfiles/filedetails/?id=2331671641&searchtext=schnightsy");
        }

        // !acmo !nightsy !ranger
        private void ExecuteFriendELO(string username, string requestedUsername)
        {
            Say("@" + username + " " + requestedUsername + "'s current elo is: "
                + HivescoreFetcher.FetchFriendELO(requestedUsername).Result);
        }

        // !xhair
        private void ExecuteCrosshair()
        {
            Say("green plus without gap");
        }

        private void ExecuteDPI()
        {
            Say("1600 dpi still figuring out in-game senses");
        }

        private void TimeUserOut(string Username)
        {
            Say("/timeout " + Username + " 1");
        }

        // !today !daily
        private void ExecuteDailyStats()
        {
            Say(hivescorePoller.GetDailyStatsMessage());
        }

        // !vm
        private void ExecuteViewModel()
        {
            Say("settings -> misc -> viewmodel" );
        }

        // !tts
        private void ExecuteSaveTTSSettings(string username, string arguments)
        {
            // TTS arguments come in the form of <Type> <Value>
            string ttsCommand = GetMessageCommand(arguments);
            string ttsArguments = GetMessageArgument(arguments);

            // Special case for user friendly commands that dont have an argument
            if (arguments.StartsWith("enable") ||arguments.StartsWith("on"))
                ttsCommand = "enable";
            else if (arguments.StartsWith("disable") || arguments.StartsWith("off"))
                ttsCommand = "disable";

            // If theres no command assume they want help
            if (ttsCommand == "" || ttsCommand == "help")
                ttsCommand = "help";

            // Get existing settings - constructor will provide defaults otherwise
            UserTTSSettings settings = MessageSpeakerSettingsManager.GetSettingsFromStorage(username);
            settings.twitchUsername = username;

            // Overwrite settings if we get a valid command
            switch (ttsCommand)
            {
                case "help":
                    Say("!tts <setting> <value>. Settings: settings, on/off, man, woman, aus, german, " +
                        "italy, uk, america, french, japanese, bog, korean, chinese, russian, " +
                        "speed (" + UserTTSSettings.minSpeed + "-" + UserTTSSettings.maxSpeed +
                        "), pitch (" + UserTTSSettings.minPitch + "-" + UserTTSSettings.maxPitch +")");
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "settings":
                    Say("@" + username + " " + JsonConvert.SerializeObject(settings));
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
                    double speed = double.TryParse(ttsArguments, out _) ? double.Parse(ttsArguments) : -200;

                    if (speed == -200)
                    {
                        Say("@" + username + " " + MessageSpeakerSettingsManager.GetSettingsFromStorage(username).ttsSettings.speakingRate.ToString() +
                            " (" + UserTTSSettings.minSpeed + "-" + UserTTSSettings.maxSpeed + ")");
                        return;
                    }

                    if (speed < UserTTSSettings.minSpeed || speed > UserTTSSettings.maxSpeed)
                        Say("@" + username + " enter a number from " + UserTTSSettings.minSpeed + "-"
                            + UserTTSSettings.maxSpeed);
                    else
                    {
                        settings.ttsSettings.SetSpeed(speed);
                        Say("speed set");
                    }
                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "pitch":
                    // If the user didn't specify a pitch then display their current pitch
                    double pitch = double.TryParse(ttsArguments, out _) ? double.Parse(ttsArguments) : -200;

                    if (pitch == -200)
                    {
                        Say("@" + username + " " + MessageSpeakerSettingsManager.GetSettingsFromStorage(username).ttsSettings.pitch.ToString() + 
                            " (" + UserTTSSettings.minPitch + "-" + UserTTSSettings.maxPitch + ")");
                        return;
                    }

                    if (pitch < UserTTSSettings.minPitch || pitch > UserTTSSettings.maxPitch)
                        Say("@" + username + " enter a number from " + UserTTSSettings.minPitch + 
                            "-" + UserTTSSettings.maxPitch);
                    else
                    {
                        settings.ttsSettings.SetPitch(pitch);
                        Say("pitch set");
                    }

                    MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
                    break;
                case "australian":
                case "australia":
                case "aus":
                case "korean":
                case "chinese":
                case "irish":
                case "ireland":
                case "german":
                case "germany":
                case "italian":
                case "italy":
                case "british":
                case "uk":
                case "american":
                case "america":
                case "french":
                case "japanese":
                case "russian": 
                case "french canadian":
                case "dialect":
                    {
                        SaveDialectSayResult(settings, ttsCommand, username);
                        break;
                    }
                case "frenchwoman":
                    {
                        GoogleTTSSettings.SetPresetVoice(username, GoogleTTSSettings.voicePresets.frenchWoman);
                        Say("dialect saved");
                        break;
                    }
                case "frenchman":
                    {
                        GoogleTTSSettings.SetPresetVoice(username, GoogleTTSSettings.voicePresets.frenchMan);
                        Say("dialect saved");
                        break;
                    }
                case "bog":
                    {
                        GoogleTTSSettings.SetPresetVoice(username, GoogleTTSSettings.voicePresets.bog);
                        Say("dialect saved");
                        break;
                    }
                case "man":
                case "woman":
                case "male":
                case "female":
                case "unspecified":
                case "neutral":
                    SaveGenderSayResult(settings, ttsCommand, username);
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

        // Used by google TTS
        private void SaveDialectSayResult(UserTTSSettings settings, string dialect, string username)
        {
           settings.ttsSettings.languageCode = GoogleTTSSettings.GetLanguageCodeFromDialect(dialect);

            if (settings.ttsSettings.languageCode != "")
                Say("dialect saved");
            else
                Say("@" + username + " Choose between australian, irish, german, italian, british, american, french, japanese");
            MessageSpeakerSettingsManager.SaveSettingsToStorage(settings);
        }

        private void SaveGenderSayResult(UserTTSSettings settings, string gender, string Username)
        {
            bool IsSaved = settings.ttsSettings.SetGender(gender);

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

        private void ExecuteTomorrow()
        {
            var quotes = new string[]
            {
                "\"The best way to predict the future is to create it.\" -Abraham Lincoln",
                "\"Education is the passport to the future, for tomorrow belongs to those who prepare for it today.\" -Malcolm X",
                "\"The past cannot be changed. The future is yet in your power.\" -Mary Pickford",
                "\"What is coming is better than what is gone.\" -Arabic Proverb",
                "\"The future depends on what we do in the present.\" -Mahatma Gandi",
                "\"Just because the past didn't turn out like you wanted it to, doesn't mean your future can't be better than you imagined.\" -Anonymous",
                "\"The future starts today, not tomorrow.\" -Pope John Paul II",
                "\"The past is your lesson. The present is your gift. The future is your motivation.\" -Anonymous",
                "\"A person can change his future by merely changing his attitude.\" -Earl Nightingale"
            };
            Say(quotes.ElementAt(new Random().Next(0, quotes.Length)));
        }
    }
}
