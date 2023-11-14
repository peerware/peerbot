using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBot.MessageSpeaker
{
    public class TTSSettings
    {

        public double speakingRate = 0.58;
        public double pitch = -3.8;
        public eGender gender = eGender.male;
        public string languageCode = "fr-CA"; // Only used by google text-to-speech API
        public string voiceName = "";

        public enum eGender
        {
            male,
            female
        }

        public bool SetGender(string gender)
        {
            switch (gender.ToLower().Trim())
            {
                case "male":
                case "man":
                    this.gender = eGender.male;
                    return true;
                    break;
                case "female":
                case "woman":
                    this.gender = eGender.female;
                    return true;
                    break;
            }

            return false;
        }

        public void SetGender(SsmlVoiceGender gender)
        {
            if (gender == SsmlVoiceGender.Male)
                this.gender = eGender.male;
            else
                this.gender = eGender.female;
        }

        public SsmlVoiceGender GetGender()
        {
            if (gender == eGender.male)
                return SsmlVoiceGender.Male;
            else
                return SsmlVoiceGender.Female;
        }

        public void SetSpeed(double value)
        {
            speakingRate = value;
        }

        public void SetPitch(double value)
        {
            pitch = value;
        }

        public static TTSSettings GetDefaultVoice()
        {
            return new TTSSettings
            {
                languageCode = "en-US",
                voiceName = "en-US-Neural2-F",
                gender = TTSSettings.eGender.female,
                speakingRate = 0.75,
                pitch = -4
            };
        }
    }
}
