using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ChatBot.MessageSpeaker
{
    /// <summary>
    /// Responsible for saving and fetching message speaker options
    /// </summary>
    public static class UserTTSSettingsManager
    {
        static object locker = new object();
        static string filePath = Config.fileSavePath + "speechsettings.txt";

        public static void SaveSettingsToStorage(UserTTSSettings messageSpeakerSettings)
        {
            lock (locker)
            {
                // First get the existing settings from storage to see if we are saving new vs overwriting
                UserTTSSettings settings = GetSettingsFromStorage(messageSpeakerSettings.twitchUsername);

                // If the setting already exists delete it then save the new settings
                if (settings.twitchUsername.Length > 0)
                {
                    List<string> allSettings = File.ReadAllLines(filePath).ToList();
                    string existingTTSSettings = "";

                    foreach (var fileSetting in allSettings)
                    {
                        if (fileSetting.Contains(settings.twitchUsername))
                            existingTTSSettings = fileSetting;
                    }
                    allSettings.Remove(existingTTSSettings);

                    File.Delete(filePath);
                    File.WriteAllLines(filePath, allSettings);
                }

                // Save the new settings
                try
                {
                    File.AppendAllText(filePath, messageSpeakerSettings.twitchUsername + ":" + JsonConvert.SerializeObject(messageSpeakerSettings) + "\n");
                    return;
                }
                catch (Exception e)
                {
                    Console.Write("Message logging failed.");
                }
            }
        }

        public static UserTTSSettings GetSettingsFromStorage(string username)
        {
            UserTTSSettings userTTSSettings = new UserTTSSettings();

            if (File.Exists(filePath))
            {
                List<string> AllSettings = File.ReadAllLines(filePath).ToList();
                foreach (var Setting in AllSettings)
                {
                    if (Setting.Contains(username))
                        userTTSSettings = JsonConvert.DeserializeObject<UserTTSSettings>(Setting.Substring(Setting.IndexOf(":") + 1));
                }
            }

            if (string.IsNullOrWhiteSpace(userTTSSettings.twitchUsername))
                userTTSSettings.ttsSettings = GetRandomVoice(userTTSSettings.twitchUsername);

            return userTTSSettings;
        }

        /// <summary>
        /// Returns a hand sculpted random ai with a name of "testvoice####"
        /// </summary>
        /// <returns></returns>
        public static TTSSettings GetRandomVoice(string username)
        {
            UserTTSSettings settings;
            Random random = new Random();

            if (File.Exists(filePath))
            {
                List<string> testVoiceSettings = File.ReadAllLines(filePath).Where(o => o.Contains("testvoice")).ToList();

                // Return a hard coded default voice if there arent any test voices
                if (testVoiceSettings.Count < 1)
                    return TTSSettings.GetDefaultVoice();

                // Return a random test voice
                string testVoiceString = testVoiceSettings[random.Next(0, testVoiceSettings.Count)];

                
                var defaultCustomVoice = JsonConvert.DeserializeObject<UserTTSSettings>(testVoiceString.Substring(testVoiceString.IndexOf(":") + 1)).ttsSettings;

                // Give users with the same default voice a custom sound
                if (username is not null)
                {
                    int randomNumber = username.Length % 5;

                    if (username.Length < 6)
                        randomNumber *= -1;

                    defaultCustomVoice.pitch += randomNumber;
                    defaultCustomVoice.speakingRate += Math.Abs((double)randomNumber / 10);
                }

                return defaultCustomVoice;
            }
            else 
                return TTSSettings.GetDefaultVoice();
        }
    }
}
