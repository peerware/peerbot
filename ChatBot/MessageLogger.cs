using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwitchLib.Client.Models;
using System.Linq;

namespace ChatBot
{
    public class MessageLogger
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "logs.txt";
        
        public static void LogMessage(ChatMessage message)
        {
            lock (locker)
            {
                try
                {
                    // Ensure the file is created to prevent errors
                    if (!File.Exists(filePath))
                        File.Create(filePath);

                    // Get the current time in this time zone (I want the text file to also be in my timezone and this should account for daylight savings)
                    TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                    File.AppendAllText(filePath,  "\n" + easternTime.ToString("F") + " " + message.Username + ": " + message.Message);
                    return;
                }
                catch (Exception e)
                {
                    Console.Write("Message logging failed.");
                }
            }
        }

        /// <summary>
        /// Returns a date time of the last message sent
        /// </summary>
        /// <returns></returns>
        public static DateTime GetTimeOfLatestMessage()
        {
            // Ensure the file is created to prevent errors
            if (!File.Exists(filePath))
                File.Create(filePath);


            List<string> FileAsLines;
            string LastLine;

            try
            {
                FileAsLines = File.ReadAllLines(filePath).ToList();
                LastLine = FileAsLines.Last();
            }
            catch (Exception ex)
            {
                return DateTime.MinValue; // This error will only occur on a fresh deployment, no need to handle it properly for now
            }

            // Read the date
            DateTime BrokenDateTime = DateTime.Now;
            string DateString;

            try
            {
                DateString = LastLine.Substring(0, (LastLine.IndexOf("AM") > 0 ? LastLine.IndexOf("AM") : LastLine.IndexOf("PM")) + 2);
                BrokenDateTime = DateTime.ParseExact(DateString, "F", null, System.Globalization.DateTimeStyles.None);
            }
            catch (Exception e)
            {
                try
                {

                    DateString = LastLine.Substring(0, LastLine.IndexOf("PM") + 2);
                    BrokenDateTime = DateTime.ParseExact(DateString, "F", null, System.Globalization.DateTimeStyles.None);
                }
                catch (Exception ex)
                {

                }
            }

            return BrokenDateTime;
        }
    }
}
