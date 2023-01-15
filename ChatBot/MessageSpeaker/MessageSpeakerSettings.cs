using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.Text;


namespace ChatBot.MessageSpeaker
{
    public class MessageSpeakerSettings
    {
        public string twitchUsername = "";
        public double speakingRate = 1.12;
        public double pitch = -1.3;
        public string languageCode = "fr-FR";
        public SsmlVoiceGender gender = Google.Cloud.TextToSpeech.V1.SsmlVoiceGender.Female;
        public bool isSpeechEnabled = true;

        public const double minPitch = -5;
        public const double maxPitch = 5;
        public const double minSpeed = 0.45;
        public const double maxSpeed = 2;

        public enum eDialects
        {
            australian,
            irish,
            italian,
            german,
            british,
            american,
            french,
            japanese,
            canadianFrench,
            korean,
            chinese,
            none
        }

        public void SetIsSpeechEnabled(string argumet)
        {
            if (argumet.StartsWith("enable"))
                isSpeechEnabled = true;
            if (argumet.StartsWith("disable"))
                isSpeechEnabled = false;
        }

        public bool SetGender(string gender)
        {   
            switch (gender)
            {
                case "man":
                    this.gender = SsmlVoiceGender.Male;
                    return true;
                    break;
                case "woman":
                    this.gender = SsmlVoiceGender.Female;
                    return true;
                    break;
                case "unspecified":
                    this.gender = SsmlVoiceGender.Unspecified;
                    return true;
                    break;
                case "neutral":
                    this.gender = SsmlVoiceGender.Neutral;
                    return true;
                    break;
                default:
                    return false;
            }
        }

        public void SetSpeed(double value)
        {
            speakingRate = value;
        }

        public void SetPitch(double value)
        {
            pitch = value;
        }

        public bool SetLanguage(string dialect)
        {
            switch (MessageSpeakerSettings.GetDialectFromString(dialect))
            {
                case eDialects.australian:
                    languageCode = "en-AU";
                    return true;
                case eDialects.chinese:
                    languageCode = "cmn-TW";
                    return true;
                case eDialects.korean:
                    languageCode = "ko-KR";
                    return true;
                case eDialects.irish:
                    languageCode = "en-IE";
                    return true;
                case eDialects.british:
                    languageCode = "en-GB";
                    return true;
                case eDialects.italian:
                    languageCode = "it-IT";
                    return true;
                case eDialects.german:
                    languageCode = "de-DE";
                    return true;
                case eDialects.american:
                    languageCode = "en-US";
                    return true;
                case eDialects.french:
                    languageCode = "fr-FR";
                    return true;
                case eDialects.japanese:
                    languageCode = "ja-JP";
                    return true;
                case eDialects.canadianFrench:
                    languageCode = "fr-CA";
                    return true;
                default:
                    return false;
            }
        }

        private VoiceSelectionParams GetVoiceParams()
        {
            VoiceSelectionParams voiceParams = new VoiceSelectionParams();
            voiceParams.LanguageCode = languageCode;
            voiceParams.SsmlGender = gender;
            voiceParams.Name = GetVoiceNameFromLanguageCode(languageCode);

            return voiceParams;
        }

        public string GetVoiceNameFromLanguageCode(string languageCode)
        {
            // Hand picked voice sounds by region - if the enum isnt covered in this list just use the default voice name
            if (gender == SsmlVoiceGender.Male)
            {
                switch (GetDialectFromLanguageCode(languageCode))
                {
                    case eDialects.australian:
                        return "en-AU-Wavenet-D";
                    case eDialects.chinese:
                        return "cmn-TW-Wavenet-C";
                    case eDialects.korean:
                        return "ko-KR-Wavenet-C";
                    case eDialects.british:
                        return "en-GB-Wavenet-D";
                    case eDialects.italian:
                        return "it-IT-Wavenet-D";
                    case eDialects.german:
                        return "de-DE-Standard-D";
                    case eDialects.american:
                        return "en-US-Standard-D";
                    case eDialects.french:
                        return "fr-FR-Wavenet-B";
                    case eDialects.japanese:
                        return "ja-JP-Wavenet-D";
                    case eDialects.canadianFrench:
                        return "fr-CA-Standard-D";
                    default:
                        return "fr-FR-Wavenet-B";
                }
            }
            else if (gender == SsmlVoiceGender.Female)
            {
                switch (GetDialectFromLanguageCode(languageCode))
                {
                    case eDialects.australian:
                        return "en-AU-Wavenet-C";
                    case eDialects.chinese:
                        return "cmn-TW-Wavenet-A";
                    case eDialects.korean:
                        return "ko-KR-Wavenet-A";
                    case eDialects.british:
                        return "en-GB-Wavenet-F";
                    case eDialects.italian:
                        return "it-IT-Wavenet-A";
                    case eDialects.german:
                        return "de-DE-Wavenet-A";
                    case eDialects.american:
                        return "en-US-Wavenet-F";
                    case eDialects.french:
                        return "fr-FR-Wavenet-A";
                    case eDialects.japanese:
                        return "ja-JP-Wavenet-B";
                    default:
                        return "fr-FR-Wavenet-A";
                }
            }
            else return "fr-FR-Wavenet-A";
        }

        public eDialects GetDialectFromLanguageCode(string languageCode)
        {
            switch (languageCode)
            {
                case "en-GB":
                    return eDialects.british;
                case "ko-KR":
                    return eDialects.korean;
                case "cmn-TW":
                    return eDialects.chinese;
                case "it-IT":
                    return eDialects.italian;
                case "de-DE":
                    return eDialects.german;
                case "en-AU":
                    return eDialects.australian;
                case "en-US":
                    return eDialects.american;
                case "fr-FR":
                    return eDialects.french;
                case "ja-JP":
                    return eDialects.japanese;
                case "fr-CA":
                    return eDialects.canadianFrench;
                default:
                    return eDialects.french;
            }
        }

        public static eDialects GetDialectFromString(string dialect)
        {
            switch (dialect.ToLower())
            {
                case "australian":
                case "australia":
                case "aus":
                    return eDialects.australian;
                case "chinese":
                    return eDialects.chinese;
                case "korean":
                    return eDialects.korean;
                case "irish":
                case "ireland":
                    return eDialects.irish;
                case "italian":
                case "italy":
                    return eDialects.italian;
                case "german":
                case "germany":
                    return eDialects.german;
                case "british":
                case "uk":
                    return eDialects.british;
                case "american":
                case "america":
                    return eDialects.american;
                case "french":
                case "france":
                    return eDialects.french;
                case "japanese":
                case "japan":
                    return eDialects.japanese;
                case "canadian french":
                    return eDialects.canadianFrench;
                default:
                    return eDialects.none;
            }
        }


        private AudioConfig GetAudioConfig()
        {
            AudioConfig audioConfig = new AudioConfig();
            audioConfig.AudioEncoding = AudioEncoding.Mp3;
            audioConfig.SpeakingRate = speakingRate;
            audioConfig.Pitch = pitch;
            return audioConfig;
        }

        public SynthesizeSpeechRequest GetSpeechRequest(string Username, string Message)
        {
            var settings = MessageSpeakerSettingsManager.GetSettingsFromStorage(Username);
            var config = settings.GetAudioConfig();
            var voiceParams = settings.GetVoiceParams();

            SynthesizeSpeechRequest request = new SynthesizeSpeechRequest();
            request.AudioConfig = config;
            request.Voice = voiceParams;

            // Build the input and add it
            SynthesisInput input = new SynthesisInput();
            input.Text = Message;
            request.Input = input;

            return request;
        }
    }
}
