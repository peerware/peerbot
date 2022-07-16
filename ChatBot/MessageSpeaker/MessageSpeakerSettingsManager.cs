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

        public enum voicePresets
        {
            frenchWoman,
            frenchMan,
            bog
        }

        /// <summary>
        /// Gives a user a handmade preconfigurated voice
        /// </summary>
        /// <param name="username"></param>
        /// <param name="voicePresets"></param>
        public static void SetPresetVoice(string username, voicePresets voicePresets)
        {
            MessageSpeakerSettings preset = new MessageSpeakerSettings();
            preset.twitchUsername = username;

            switch (voicePresets)
            {
                case voicePresets.frenchWoman:
                    preset.speakingRate = 1.16;
                    preset.pitch = -1.3;
                    preset.gender = Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female;
                    preset.isSpeechEnabled = true;
                    preset.languageCode = "fr-FR";
                    break;
                case voicePresets.bog:
                    preset.speakingRate = 0.57;
                    preset.pitch = -2.5;
                    preset.gender = Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Male;
                    preset.isSpeechEnabled = true;
                    preset.languageCode = "fr-CA";
                    break;
                case voicePresets.frenchMan:
                    preset.speakingRate = 1.17;
                    preset.pitch = -2;
                    preset.gender = Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Male;
                    preset.isSpeechEnabled = true;
                    preset.languageCode = "fr-FR";
                    break;
            }

            SaveSettingsToStorage(preset);
        } 

        public static void SaveSettingsToStorage(MessageSpeakerSettings messageSpeakerSettings)
        {
            lock (locker)
            {
                // First get the existing settings from storage to see if we are saving new vs overwriting
                MessageSpeakerSettings settings = GetSettingsFromStorage(messageSpeakerSettings.twitchUsername);

                // If the setting already exists delete it then save the new settings
                if (settings.twitchUsername.Length > 0)
                {
                    List<string> allSettings = File.ReadAllLines(filePath).ToList();
                    string existingSettingsLine = "";

                    foreach (var setting in allSettings)
                    {
                        if (setting.Contains(settings.twitchUsername))
                            existingSettingsLine = setting;
                    }
                    allSettings.Remove(existingSettingsLine);

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

        public static MessageSpeakerSettings GetSettingsFromStorage(string Username)
        {
            MessageSpeakerSettings Output = new MessageSpeakerSettings();

            if (File.Exists(filePath))
            {
                List<string> AllSettings = File.ReadAllLines(filePath).ToList();
                foreach (var Setting in AllSettings)
                {
                    if (Setting.Contains(Username))
                        Output = JsonConvert.DeserializeObject<MessageSpeakerSettings>(Setting.Substring(Setting.IndexOf(":") + 1));
                }
            }

            return Output;
        }
    }
}
