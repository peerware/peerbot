using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace YoutubeClient.Models
{
    public class GoogleTTSVoice 
    {
        public GoogleTTSVoice() { }

        public int id { get; set; } = 0;

        public string languageCode { get; set; } = "";

        public string languageName { get; set; } = "";

        public string username { get; set; } = "";

        public double speed { get; set; } = 0;

        public double pitch { get; set; } = 0;

        public string message { get; set; } = "";

        public SsmlVoiceGender gender { get; set; } = SsmlVoiceGender.Unspecified;


        public string GetDisplayName()
        {
            string genderLabel = "";

            if (gender == SsmlVoiceGender.Male)
                genderLabel = "m";
            else
                genderLabel = "f";

            return new CultureInfo(languageCode).EnglishName + " (" + genderLabel + ")";
        }
    }
}
