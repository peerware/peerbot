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

        public static int AddQuote(string message)
        {
            lock (locker)
            {
                try
                {
                    File.AppendAllText(filePath, "\n" + message);
                    return File.ReadAllLines(filePath).ToList().Count;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Quote adding failed.");
                    return 0;
                }
            }
            
        }

        public static string GetRandomQuote()
        {
            lock (locker)
            {
                try
                {
                    // Ensure the file is created to prevent errors
                    if (!File.Exists(filePath))
                        File.Create(filePath);

                    List<string> quotes = File.ReadAllLines(filePath).ToList();
                    int quoteNumber = new Random().Next(0, quotes.Count);

                    if (quotes.Count == 0)
                        return "no quotes";
                    else
                        return "\"" + quotes.ElementAt(quoteNumber) + "\"" + " " + quoteNumber;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Quote fetching failed.");
                    return "no quotes";
                }
            }
        }

        public static string GetNumberedQuote(int quoteNumber)
        {
            lock (locker)
            {
                try
                {
                    // Ensure the file is created to prevent errors
                    if (!File.Exists(filePath))
                        File.Create(filePath);

                    List<string> quotes = File.ReadAllLines(filePath).ToList();

                    if (quotes.Count == 0)
                        return "no quotes";
                    else
                    {
                        try
                        {
                            string quote = quotes[quoteNumber - 1];
                            return "\"" + quote + "\"";
                        }
                        catch (Exception e)
                        {
                            return "quote doesnt exist";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Numbered quote fetching failed.");
                    return "no quotes";
                }
            }
        }
    }
}
