using TwitchLib.Client.Models;
using Google.Cloud.TextToSpeech.V1;
using System;
using System.Media;
using System.IO;
using NAudio.Wave;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ChatBot.MessageSpeaker
{
    public class MessageSpeaker
    {
        public MessageSpeaker()
        {
            // Auths into google (probably a better way but this is fast)
            string credential_path = Config.fileSavePath + "peerbot-329501-7bffcbd28a99.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
        }

        public TextToSpeechClient client = TextToSpeechClient.Create();

        // Speaks a message using the user's saved settings (otherwise use custom defaults)
        public void SpeakMessage(string username, string message)
        {
            // Filter messages we don't want spoken
            if (!IsMessageAllowedSpoken(message))
                return;

            // Get the users settings and figure out which TTS platform to use
            UserTTSSettings userTTSSettings = UserTTSSettingsManager.GetSettingsFromStorage(username);
            if (!userTTSSettings.isSpeechEnabled) // Filter users who have tts disabled
                return;

            if (userTTSSettings.TTSType == UserTTSSettings.eTTSType.google)
                SpeakGoogleMessage(userTTSSettings, message);
        }
        
        /// <summary>
        /// Plays a google TTS message using NAudio
        /// </summary>
        /// <param name="ttsSettings"></param>
        /// <param name="message"></param>
        public void SpeakGoogleMessage(UserTTSSettings userTTSSettings, string message)
        {
            try
            {
                PlayAudioFromStream(GoogleTTSSettings.GetVoiceAudio(userTTSSettings.twitchUsername, message, client));
            }
            catch (Exception e)
            {
                Console.Write("\n" + e.Message);
            }
        }

        private void PlayAudioFromStream(MemoryStream stream)
        {
            WaveFileReader reader = new WaveFileReader(new MemoryStream(stream.ToArray()));

            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(reader);
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
