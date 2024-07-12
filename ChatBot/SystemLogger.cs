using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace ChatBot
{
    public static class SystemLogger
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "systemlogs.txt";

        public static void Log(string error)
        {
            lock (locker)
            {
                try
                {
                    // Get the current time in this time zone (I want the text file to also be in my timezone and this should account for daylight savings)
                    TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                    File.AppendAllText(filePath, "\n" +
                        "######################################################" +
                        "\n" + easternTime.ToString("F") + ": " + error);

                    Console.WriteLine("\n" + easternTime.ToString("F") + ": " + error + "\n");
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("System logging failed.");
                }
            }
        }
    }
}