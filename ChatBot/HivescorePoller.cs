
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
            {
                output += string.Format(" -{0}", Math.Abs(HivescoreDifference));

                if (HivescoreDifference <= -15)
                    output += " TrollDespair ";
            }
            else
            {
                output += string.Format(" +{0}", HivescoreDifference);

                if (HivescoreDifference <= 5)
                    output += " AlienPls ";
                else if (HivescoreDifference > 19)
                    output += " pepeRun ";
            }

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
    }
}
