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

        public enum eGender
        {
            male,
            female
        }

        public bool SetGender(string gender)
        {
            switch (gender)
            {
                case "man":
                    this.gender = eGender.male;
                    return true;
                    break;
                case "woman":
                    this.gender = eGender.female;
                    return true;
                    break;
            }

            return false;
        }

        public void SetSpeed(double value)
        {
            speakingRate = value;
        }

        public void SetPitch(double value)
        {
            pitch = value;
        }
    }
}
