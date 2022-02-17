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
        string settingsFilePath = @"C:\Users\Peer\Desktop\desktop\output.mp3";

        // Speaks a message using the user's saved settings (otherwise use custom defaults)
        public void SpeakMessage(ChatMessage message)
        {
            // Filter messages we don't want spoken
            if (!IsMessageAllowedSpoken(message.Message))
                return;

            // Filter users who have tts disabled
            var settings = MessageSpeakerSettingsManager.GetSettingsFromStorage(message.Username);
            if (!settings.isSpeechEnabled)
                return;

            SynthesizeSpeechRequest request = settings.GetSpeechRequest(message.Username, message.Message);

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

                // This is an easy and tested safe way of playing mp3 files (its tricky)
                using (var audioFile = new AudioFileReader(settingsFilePath))
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
            catch (Exception e)
            {
                Console.Write("\n" + e.Message);
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
