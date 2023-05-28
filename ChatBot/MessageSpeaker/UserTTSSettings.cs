using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.Text;


namespace ChatBot.MessageSpeaker
{
    // hold all the tts settings for a user
    public class UserTTSSettings
    {
        public string twitchUsername = "";
        public bool isSpeechEnabled = true;
        public TTSSettings ttsSettings = new TTSSettings();
        public eTTSType TTSType = eTTSType.google;

        public const double minPitch = -5;
        public const double maxPitch = 5;
        public const double minSpeed = 0.45;
        public const double maxSpeed = 2;


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
