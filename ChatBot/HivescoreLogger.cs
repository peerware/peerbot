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

                        // Append ticks:hivescore to end of file
                        File.AppendAllText(filePath, formattedHivescore + "\n");
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Hivescore logging failed. " + e.Message.Substring(0, 50));
                    }
                }
            }
        }
    }
}
