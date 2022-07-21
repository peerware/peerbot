using System;
using System.IO;

namespace ChatBot
{
    public class IdeaLogger
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "ideas.txt";

        public static void LogIdea(string username, string message)
        {
            lock (locker)
            {
                try
                {
                    string idea = message.Substring(message.IndexOf(" "));

                    // Get the current time in this time zone (I want the text file to also be in my timezone and this should account for daylight savings)
                    TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                    File.AppendAllText(filePath, easternTime.ToString("F") + " " + username + ": " + message + "\n");
                    return;
                }
                catch (Exception e)
                {
                    Console.Write("Idea logging failed.");
                }
            }
        }
    }
}