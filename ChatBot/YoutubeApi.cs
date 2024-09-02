using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    public static class YoutubeAPI
    {
        public static async Task<string> GetVideoName(string URL)
        {
            try
            {
                var youtube = new YoutubeExplode.YoutubeClient();
                var video = await youtube.Videos.GetAsync(URL);

                return video.Title;
            }
            catch (Exception e) { }
            return null;
        }
        public static async Task<int> GetVideoLength(string URL)
        {
            try
            {
                var youtube = new YoutubeExplode.YoutubeClient();
                var video = await youtube.Videos.GetAsync(URL);

                int totalSeconds = (video.Duration?.Seconds ?? 0)
                    + ((video.Duration?.Minutes ?? 0) * 60)
                    + ((video.Duration?.Hours ?? 0) * 60 * 60);
                return totalSeconds;
            }
            catch (Exception e) { }
            return 0;
        }
    }
}
