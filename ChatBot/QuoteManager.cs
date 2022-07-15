using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwitchLib.Client.Models;
using System.Linq;

namespace ChatBot
{
    public class QuoteManager
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "quotes.txt";

        public static void AddQuote(string message)
        {
            lock (locker)
            {
                try
                {
                    File.AppendAllText(filePath, "\n" + message);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Quote adding failed.");
                }
            }
            
        }
        public static string GetRandomQuote()
        {
            lock (locker)
            {
                try
                {
                    List<string> quotes = File.ReadAllLines(filePath).ToList();

                    if (quotes.Count == 0)
                        return "no quotes";
                    else
                        return "\"" + quotes.ElementAt(new Random().Next(0, quotes.Count)) + "\"";
                }
                catch (Exception e)
                {
                    Console.WriteLine("Quote fetching failed.");
                    return "no quotes";
                }
            }
            
        }
    }
}
