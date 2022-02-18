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
        static string filePath = @"C:\Users\Peer\Desktop\hivescoredata.txt";

        public static void LogHivescore(int hivescore)
        {
            while (true)
            {
                lock (locker)
                {
                    // Milliseconds:hivescore
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
        public static int GetHivescoreChange(DateTime minimumDate)
        {
            List<string> allLoggedScores = new List<string>();

            if (File.Exists(filePath))
                allLoggedScores = File.ReadAllLines(filePath).ToList();

            // Get the hivescore values as a list
            List<int> minDateHivescores = allLoggedScores.Where(o => long.Parse(o.Substring(0, o.IndexOf(":"))) >= minimumDate.Ticks).Select(o => int.Parse(o.Substring(o.IndexOf(":") + 1))).ToList();

            if (minDateHivescores.Count == 0)
                return 0;
            else if (minDateHivescores.Count == 1)
                return 0;
            else
                return minDateHivescores.Last() - minDateHivescores.First();
        }
    }
}
