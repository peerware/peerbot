using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace ChatBot
{
    public class HivescoreFetcher
    {
        static string hivescorePath = Config.fileSavePath + "hivescoredata.txt";
        static string tdPath = Config.fileSavePath + "tddata.txt";

        public static async Task<string> FetchHivescore()
        {
            HttpClient httpClient = new HttpClient();
            string ResultString = await httpClient.GetStringAsync("http://hive2.ns2cdt.com/api/players/87160873");

            return ResultString.Substring(ResultString.IndexOf("skill") + 7, 4);
        }

        public static async Task<string> FetchTDELO()
        {
            HttpClient httpClient = new HttpClient();
            string ResultString = await httpClient.GetStringAsync("http://hive2.ns2cdt.com/api/players/87160873");

            return ResultString.Substring(ResultString.IndexOf("td_skill") + 10, 4);
        }

        /// <summary>
        /// Returns an ELO if the friends steamID is added and 0 if their steamID wasnt found 
        /// </summary>
        /// <returns></returns>
        public static async Task<string> FetchFriendELO(string username)
        {
            int steamID = 0;

            switch (username.Trim().ToLower())
            {
                case "ranger":
                    steamID = 123819912;
                    break;
                case "acmo":
                    steamID = 123819912;
                    break;
                case "nightsy":
                    steamID = 73407070;
                    break;
                case "snowblind":
                    steamID = 3095193;
                    break;
                case "robp":
                    steamID = 59625362;
                    break;
                case "kittn":
                    steamID = 84276590;
                    break;
                case "golden":
                    steamID = 344054;
                    break;
                case "schu":
                    steamID = 181662;
                    break;
                default:
                    return "0";
            }
            
            HttpClient httpClient = new HttpClient();
            string ResultString = await httpClient.GetStringAsync("http://hive2.ns2cdt.com/api/players/" + steamID);

            return ResultString.Substring(ResultString.IndexOf("skill") + 7, 4);
        }


        /// <summary>
        /// Gets a difference in hivescore from after the provided date
        /// </summary>
        /// <param name="minimumDate"></param>
        /// <returns></returns>
        private static List<int> GetMinDateHivescores(DateTime minimumDate, HivescorePoller.ePollingType pollingType)
        {
            List<string> allLoggedScores = new List<string>();

            string filePath = "";

            if (pollingType == HivescorePoller.ePollingType.hivescore)
                filePath = hivescorePath;
            else if (pollingType == HivescorePoller.ePollingType.td)
                filePath = tdPath;

            if (File.Exists(filePath))
                allLoggedScores = File.ReadAllLines(filePath).ToList();

            // Get the hivescores that come after the min date as a list
            List<int> minDateHivescores = new List<int>();
            foreach (var datedHivescore in allLoggedScores)
            {
                long hivescoreDate = long.Parse(datedHivescore.Substring(0, datedHivescore.IndexOf(':')));
                if (hivescoreDate >= minimumDate.Ticks)
                    minDateHivescores.Add(int.Parse(datedHivescore.Substring(datedHivescore.IndexOf(':') + 1)));
            }

            return minDateHivescores;
        }

        /// <summary>
        /// Gets a difference in hivescore from after the provided date
        /// </summary>
        /// <param name="minimumDate"></param>
        /// <returns></returns>
        public static string GetHivescoreChange(DateTime minimumDate, HivescorePoller.ePollingType pollingType)
        {
            // Get the hivescores that come after the min date as a list
            List<int> minDateHivescores = GetMinDateHivescores(minimumDate, pollingType);

            if (minDateHivescores.Count == 0)
                return "+0";
            else if (minDateHivescores.Count == 1)
                return "+0";
            else
            {
                int difference = minDateHivescores.Last() - minDateHivescores.First();
                return difference > 0 ? "+" + difference.ToString() : difference.ToString(); // Adds a plus sign for display
            }
        }

        /// <summary>
        /// Gets a formatted hivescore message comparing present value with the closest value from the date provided
        /// </summary>
        /// <param name="minimumDate"></param>
        /// <returns></returns>
        public static string GetOldHivescoreMessage(DateTime minimumDate, HivescorePoller.ePollingType pollingType)
        {
            // Get the hivescores that come after the min date as a list
            List<int> minDateHivescores = GetMinDateHivescores(minimumDate, pollingType);

            if (minDateHivescores.Count == 0)
                return "not enough data :(";
            else if (minDateHivescores.Count == 1)
                return "not enough data :(";
            else
            {
                // Formats a date as August 17
                string formattedDate = minimumDate.ToString("M");

                // Add a plus sign for display
                int scoreDifference = minDateHivescores.Last() - minDateHivescores.First();
                string formatedDifference = scoreDifference > 0 ? "+" + scoreDifference.ToString() : scoreDifference.ToString();

                // Build out the final message
                string formattedMessage = formattedDate + ": " + minDateHivescores.First()
                    + " Now: " + minDateHivescores.Last() + " (" + formatedDifference + ")";

                return formattedMessage;
            }
        }
    }
}
