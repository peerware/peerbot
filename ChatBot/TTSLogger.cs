using ChatBot.MessageSpeaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChatBot
{
    public class TTSLogger
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "ttsStats.txt";
        public static int TTSTests = 0;
        public static int TTSCharacters = 0;
        public static List<MonthlyTTSStat> MonthlyStats = new List<MonthlyTTSStat>();

        public static void LogTTSStats(int numCharacters)
        { // todo need to check whether google is counting whitespace characters
            UpdateMonthlyTTSStats(numCharacters);

            lock (locker)
            {
                try
                {
                    // Get the monthly stats for this month
                    MonthlyTTSStat thisMonth = MonthlyStats.Find(o => o.Year == DateTime.Now.Year
                                                                    && o.Month == DateTime.Now.Month);

                    File.AppendAllText(filePath, 
                        thisMonth.Month + " " + thisMonth.Year
                        + ": # of tests:  " + TTSTests 
                        + " # of TTS Characters this month$" + thisMonth.NumCharacters + "\n");
                    return;
                }
                catch (Exception e)
                {
                    SystemLogger.Log("Failed to log tts stats. " + e);
                    Console.Write("TTS Stat logging failed.");
                }
            }
        }

        /// <summary>
        /// Updates the number of TTS Characters used this month.
        /// Handles trying to populate memory with log data 
        /// </summary>
        private static void UpdateMonthlyTTSStats(int characters)
        {
            DateTime Today = DateTime.Now;
            MonthlyTTSStat thisMonth = MonthlyStats.FirstOrDefault(o => o.Month == Today.Month
                && o.Year == Today.Year);
            
            if (thisMonth != null)
                thisMonth.NumCharacters += characters;
            else if (!File.Exists(filePath))
            {
                thisMonth = new MonthlyTTSStat
                {
                    NumCharacters = characters,
                    Month = Today.Month,
                    Year = Today.Year,
                    NumTests = 0
                };

                MonthlyStats.Add(thisMonth);
            } else {
                try
                {
                    lock (locker)
                    {
                        List<string> fileData = File.ReadAllText(filePath).Split('\n').ToList();

                        string monthEntry = fileData.LastOrDefault(o =>
                        o.Contains(Today.Month + " " + Today.Year));

                        if (monthEntry != null)
                        {
                            string numtext = monthEntry.Replace("\n", "").Trim().Substring(monthEntry.IndexOf("$") + 1);
                            int numCharacters = 0;

                            if (int.TryParse(numtext, out numCharacters))
                                AddMonthlyStat(characters + numCharacters);
                            else
                                AddMonthlyStat(characters);
                        }
                        else
                            AddMonthlyStat(characters);
                    }
                }
                catch (Exception e)
                {
                    SystemLogger.Log("Failed to fetch tts stats in TTSLogger.PopulateMonthlyTTSCharacters " + e);
                }
            }
        }

        private static void AddMonthlyStat(int numCharacters)
        {
            MonthlyTTSStat thisMonth = new MonthlyTTSStat
            {
                NumCharacters = numCharacters,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                NumTests = 0
            };
            MonthlyStats.Add(thisMonth);
        }
    }
}