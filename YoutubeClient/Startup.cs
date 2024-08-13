using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeClient.Hubs;
using ChatBot.MessageSpeaker;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using TwitchLib.Client.Events;
using Google.Cloud.TextToSpeech.V1;
using YoutubeClient.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using TwitchLib.Client.Models;

namespace YoutubeClient
{
    public class Startup
    {
        /// <summary>
        /// integrate message receiver in a way thats accessible across the whole application
        /// </summary>
        public IConfiguration Configuration { get; }
        private static MessageReceiver messageReceiver = new MessageReceiver();
        private static IHubContext<ChatHub> chatHub;
        public static TextToSpeechClient ttsClient = GoogleTTSSettings.GetTTSClient();
        public static List<GoogleTTSVoice> GoogleTTSVoices = new List<GoogleTTSVoice>();
        private static DateTime? lastSongRequest = null;
        public static int SongRequestDelay = 90;


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // SignalR should be last
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHubContext<ChatHub> chatHub)
        {
            try
            {
                app.UseStaticFiles();
                messageReceiver.twitchClient.OnMessageReceived += Client_OnMessageReceived;

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });


                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    //app.UseHsts();
                }
                //app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<ChatHub>("/chatHub");

                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    //endpoints.MapGet("/", () => "10.0.0.100");
                });

                Startup.chatHub = chatHub;
                PopulateVoices();
            }
            catch (Exception e)
            {
                SystemLogger.Log($"Failed to setup middleware in Startup.cs/Configure\n{e.ToString()}");
            }
        }
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string chatMessage = e.ChatMessage.Message.Trim();

            // Log the incoming message
            MessageLogger.LogMessage(e.ChatMessage);

            // Don't do anything with bot messages
            if (e.ChatMessage.DisplayName == Config.botUsername)
                return;

            // If the message is spam, timeout the user to delete their message and don't do anything else
            if (MessageFilter.IsMessageSpam(e.ChatMessage.Message, e.ChatMessage.IsFirstMessage))
            {
                MessageFilter.TimeoutUser(e.ChatMessage.UserId);
                return;
            }

            // Execute any commands
            if (e.ChatMessage.Message.StartsWith("!"))
                messageReceiver.messageExecutor.ExecuteMessage(e.ChatMessage);
           
            if (chatMessage.ToLower().StartsWith("!sr"))
                TryPlaySong(chatMessage);

            // Speak the message if its not a link or command
            if (!chatMessage.StartsWith("!") && !MessageFilter.IsURLInsideMessage(chatMessage))
                SendMessageAudioToClients(e.ChatMessage.Username, chatMessage);

            return;
        }

        public static void TryPlaySong(string chatMessage)
        {
            // Try to make sure videos play from the beginning
            if (!chatMessage.Contains("&"))
                chatMessage = chatMessage.TrimEnd() + "&t=0";

            double? timeSinceLastRequest = (lastSongRequest is null) ?
                0 : (DateTime.Now - lastSongRequest)?.TotalSeconds;

            if (chatMessage.Length > 8 &&
                (lastSongRequest is null || SongRequestDelay <= timeSinceLastRequest))
            {
                lastSongRequest = DateTime.Now;
                chatHub.Clients.All.SendAsync("ReceiveSongRequest", chatMessage.Replace("!sr", "").Trim());
            }
            else
            {
                string roundedTimeRemaining = (SongRequestDelay - timeSinceLastRequest)?.ToString("N0");
                string output = $"you may request a new song in {roundedTimeRemaining} seconds";
                messageReceiver.messageExecutor.Say(output);
            }
        }

        private void PopulateVoices()
        {
            try
            {
                var response = ttsClient.ListVoices(new Google.Cloud.TextToSpeech.V1.ListVoicesRequest { });

                for (int i = 0; i < response.Voices.Count; i++)
                {
                    GoogleTTSVoices.Add(new GoogleTTSVoice
                    {
                        id = i,
                        languageName = response.Voices[i].Name,
                        languageCode = response.Voices[i].LanguageCodes[0],
                        gender = response.Voices[i].SsmlGender
                    });
                }
            }
            catch (Exception e)
            {
                SystemLogger.Log($"Failed to populate voices from Startup.cs\n{e.ToString()}");
            }
        }

        /// <summary>
        /// Plays audio in the browser when recieving a message
        /// </summary>
        /// <returns></returns>
        public void SendMessageAudioToClients(string username, string message)
        {
            try
            {
                MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
                    .GetVoiceAudio(username, message, ttsClient).ToArray());

                // If the stream wasn't filled with data then discard the stream
                if (audioMemoryStream.Length < 1)
                    return;

                chatHub.Clients.All.SendAsync("ReceiveMessageAudio", System.Convert.ToBase64String(audioMemoryStream.ToArray()));
            } 
            catch (Exception ex) 
            {
                SystemLogger.Log($"Failed to get message audio from Startup.cs\n{ex.ToString()}");
            }
            return;
        }
    }
}
