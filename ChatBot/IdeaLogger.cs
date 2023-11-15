using System;
using System.Collections.Generic;
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

        public static string GetFormattedSuccessMessage(string username)
        {
            lock (locker)
            {
                try
                {
                    // Get the number of lines in the ideas.txt file
                    string[] quotes = File.ReadAllLines(filePath);
                    return "@" + username + " thank you for the idea yours is position " + quotes.Length + " in the queue";
                }
                catch (Exception e)
                {
                    Console.Write("Ideas are broken :(");
                    return "Ideas are broken :(";
                }
            }
        }

        public static int GetNumberOfIdeas()
        {
            lock (locker)
            {
                try
                {
                    // Get the number of lines in the ideas.txt file
                    return File.ReadAllLines(filePath).Length;
                }
                catch (Exception e)
                {
                    Console.Write("Ideas are broken :(");
                    return -1;
                }
            }
        }
    }
}