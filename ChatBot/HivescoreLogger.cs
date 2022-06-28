using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ChatBot
{
    public class HivescoreLogger
    {
        static object locker = new object();
        static string hivescorePath = Config.fileSavePath + "hivescoredata.txt";
        static string tdPath = Config.fileSavePath + "tddata.txt";

        public static void LogHivescore(int hivescore, HivescorePoller.ePollingType pollingType)
        {
            string filePath = "";

            if (pollingType == HivescorePoller.ePollingType.hivescore)
                filePath = hivescorePath;
            else if (pollingType == HivescorePoller.ePollingType.td)
                filePath = tdPath;

            while (true)
            {
                lock (locker)
                {
                    // ticks:hivescore
                    string formattedHivescore = DateTime.Now.Ticks + ":" + hivescore;

                    // Log to computer 
                    try
                    {
                        // Create the file if it doesnt exist
                        if (!File.Exists(filePath))
                            File.Create(filePath);

                        // Append remaining text
                        File.AppendAllText(filePath, formattedHivescore + "\n");
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.Write("Hivescore logging failed.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of hivescores that come after the provided date
        /// </summary>
        /// <param name="minimumDate"></param>
        /// <returns></returns>
        public static int GetHivescoreChange(DateTime minimumDate, HivescorePoller.ePollingType pollingType)
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

            if (minDateHivescores.Count == 0)
                return 0;
            else if (minDateHivescores.Count == 1)
                return 0;
            else
                return minDateHivescores.Last() - minDateHivescores.First();
        }
    }
}
