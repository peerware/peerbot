using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace ChatBot
{
    public class HivescoreFetcher
    {
        public static async Task<string> FetchHivescore()
        {
            HttpClient httpClient = new HttpClient();
            string ResultString = await httpClient.GetStringAsync("http://hive2.ns2cdt.com/api/players/87160873");

            return ResultString.Substring(ResultString.IndexOf("skill") + 7, 4);
        }
    }
}
