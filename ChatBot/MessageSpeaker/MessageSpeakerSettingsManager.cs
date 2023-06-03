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
    class MessageSpeakerSettingsManager
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
                userTTSSettings.ttsSettings = GoogleTTSSettings.GetDefaultVoice();

            return userTTSSettings;
        }
    }
}
