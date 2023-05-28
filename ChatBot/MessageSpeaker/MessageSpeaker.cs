using TwitchLib.Client.Models;
using Google.Cloud.TextToSpeech.V1;
using System;
using System.Media;
using System.IO;
using NAudio.Wave;
using System.Threading;

namespace ChatBot.MessageSpeaker
{
    public class MessageSpeaker
    {
        TextToSpeechClient client = TextToSpeechClient.Create();
        Mp3FileReader reader = null;
        WaveOut waveOut = null;
        string settingsFilePath = Config.fileSavePath + "output.mp3";

        // Speaks a message using the user's saved settings (otherwise use custom defaults)
        public void SpeakMessage(ChatMessage message)
        {
            // Filter messages we don't want spoken
            if (!IsMessageAllowedSpoken(message.Message))
                return;

            // Get the users settings and figure out which TTS platform to use
            UserTTSSettings userTTSSettings = MessageSpeakerSettingsManager.GetSettingsFromStorage(message.Username);
            if (!userTTSSettings.isSpeechEnabled) // Filter users who have tts disabled
                return;

            if (userTTSSettings.TTSType == UserTTSSettings.eTTSType.google)
                SpeakGoogleMessage(userTTSSettings, message.Message);
        }

        public void SpeakGoogleMessage(UserTTSSettings userTTSSettings, string message)
        {

            SynthesizeSpeechRequest request = GoogleTTSSettings.GetSpeechRequest(message, userTTSSettings);

            try
            {
                var response = client.SynthesizeSpeech(request);

                // Write the response to the output file.
                using (var output = File.Create(settingsFilePath))
                {
                    response.AudioContent.WriteTo(output);
                }

                var ms = new MemoryStream();
                response.AudioContent.WriteTo(ms);

                PlayAudioFromFile(settingsFilePath);
            }
            catch (Exception e)
            {
                Console.Write("\n" + e.Message);
            }
        }

        private void PlayAudioFromFile(string filePath)
        {
            // This is an easy and tested safe way of playing mp3 files (its tricky)
            using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Returns false if the message shouldnt be spoken
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsMessageAllowedSpoken(string message)
        {
            return message.StartsWith("!") || message.StartsWith("http") || message.StartsWith("www.") ? false : true;
        }
    }
}
