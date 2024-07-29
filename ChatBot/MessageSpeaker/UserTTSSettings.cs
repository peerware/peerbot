using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.Text;


namespace ChatBot.MessageSpeaker
{
    // Holds all the tts settings for a user
    public class UserTTSSettings
    {
        public string twitchUsername = "";
        public bool isSpeechEnabled = true;
        public TTSSettings ttsSettings = new TTSSettings();


        public enum eTTSType
        {
            google
        }

        public enum ttsGender
        {
            male,
            female
        }

        public void SetIsSpeechEnabled(string argumet)
        {
            if (argumet.StartsWith("enable"))
                isSpeechEnabled = true;
            if (argumet.StartsWith("disable"))
                isSpeechEnabled = false;
        }
    }
}
