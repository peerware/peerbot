
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;

namespace ChatBot
{
    public class HivescorePoller
    {
        private Task PollingTask;
        private List<int> PreviousHivescores = new List<int>();
        private TwitchClient client;

        public HivescorePoller(TwitchClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Polls the hivescore 3.0 api: http://hive2.ns2cdt.com/api/players/87160873
        /// </summary>
        public void BeginPolling()
        {
            PollingTask = Task.Factory.StartNew(() =>
            {
                // Never stop polling
                while (true)
                {
                    Thread.Sleep(1000 * 30); // 30 seconds                                                                      

                    int freshHivescore = int.Parse(HivescoreFetcher.FetchHivescore().Result);

                    // If we got a new hivescore send a message
                    if (PreviousHivescores.Count > 0 && !PreviousHivescores.Last().Equals(freshHivescore))
                    {
                        HivescoreLogger.LogHivescore(freshHivescore);
                        client.SendMessage(Config.channelUsername, GetDisplayMessage(PreviousHivescores.Last(), freshHivescore));
                        PreviousHivescores.Add(freshHivescore);
                    }
                    else if (PreviousHivescores.Count == 0)
                    {
                        // Save the first hivescore we get without doing anything with it
                        PreviousHivescores.Add(freshHivescore);
                    }
                }
            });
        }
        
        /// <summary>
        /// Takes two hivescores and formats a message
        /// </summary>
        /// <returns></returns>
        public string GetDisplayMessage(int PreviousHivescore, int CurrentHivescore)
        {
            // Create a string with the date and current elo
            string output = DateTime.Now.ToString("T") + " " + string.Format(" updated elo: {0}", CurrentHivescore);

            // Stylize the message for aesthetic depending on magnitude/whether its positive/negative
            int HivescoreDifference = CurrentHivescore - PreviousHivescore;

            if (HivescoreDifference < 0)
                output += string.Format(" -{0}", Math.Abs(HivescoreDifference)) + " " + GetLoserEmote() + " ";
            else
                output += string.Format(" +{0}", HivescoreDifference) + " " + GetWinnerEmote() + " ";

            return output;
        }

        /// <summary>
        /// Returns a formatted message containing daily hivescore statistics
        /// </summary>
        /// <returns></returns>
        public string GetDailyStatsMessage()
        {
            if (PreviousHivescores.Count == 0) // Stops an exception from happening
                return "No data today Sadge";

            if (PreviousHivescores.Count == 1)
                return "No data today Sadge";
            else
            {
                string OutputString = "";

                int HivescoreDifference = PreviousHivescores.Last() - PreviousHivescores.First();

                if (HivescoreDifference > 1)
                    OutputString = "+" + Math.Abs(HivescoreDifference) + " elo in " + (PreviousHivescores.Count - 1) + " games";
                else if (HivescoreDifference < 1)
                    OutputString = "-" + Math.Abs(HivescoreDifference) + " elo in " + (PreviousHivescores.Count - 1) + " games";
                else
                    OutputString = "no elo change in " + 0 + " games";

                return OutputString;
            }
        }
        public string GetWinnerEmote()
        {

            Random rnd = new Random();
            int randomNumber = rnd.Next(1, 2); // creates a number between the left value and (right value - 1)

            switch (randomNumber)
            {
                case 1:
                    return "hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway hoSway";
                case 2:
                    return "lickL";
                case 3:
                    return "AlienPls";
                case 4:
                    return "BoneZone";
                case 6:
                    return "AlienPls";
                default:
                    return "";
            }
        }

        public string GetLoserEmote()
        {
            Random rnd = new Random();
            int randomNumber = rnd.Next(1, 20); // creates a number between the left value and (right value + 1)

            switch (randomNumber)
            {
                case 1:
                    return "pepeMeltdown";
                case 2:
                    return "pepeMeltdown";
                case 3:
                    return "Sadge";
                case 4:
                    return "mericCat";
                case 6:
                    return "TrollDespair";
                case 7:
                    return "AlienPls";
                case 8:
                    return "Sadge";
                case 9:
                    return "pepeSadJam";
                case 10:
                    return "peepoSad";
                case 11:
                    return "Hmm";
                case 12:
                    return "FeelsDankMan";
                case 13:
                    return "Flushed";
                case 14:
                    return "widepeepoSad";
                case 15:
                    return "KEKW";
                default:
                    return "";
            }
        }
    }
}
