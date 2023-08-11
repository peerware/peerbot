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

namespace YoutubeClient
{
    public class Startup
    {
        /// <summary>
        /// integrate message receiver in a way thats accessible across the whole application
        /// </summary>
        public IConfiguration Configuration { get; }
        private MessageReceiver messageReceiver = new MessageReceiver();
        private IHubContext<ChatHub> chatHub;
        private TextToSpeechClient ttsClient;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHubContext<ChatHub> chatHub)
        {
            // Setup google TTS environment variable
            string credential_path = Config.fileSavePath + "peerbot-329501-7bffcbd28a99.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);

            // Setup google tts events and objects
            messageReceiver.twitchClient.OnMessageReceived += Client_OnMessageReceived;
            this.chatHub = chatHub;
            this.ttsClient = GoogleTTSSettings.GetTTSClient();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            GetMessageAudio(e.ChatMessage.Username, e.ChatMessage.Message);
        }

        /// <summary>
        /// Plays audio in the browser when recieving a message
        /// </summary>
        /// <returns></returns>
        public void GetMessageAudio(string username, string message)
        {
            MemoryStream audioMemoryStream = new MemoryStream(GoogleTTSSettings
                .GetVoiceAudio(username, message, ttsClient).ToArray());

            long audioLength = audioMemoryStream.Length;

            chatHub.Clients.All.SendAsync("ReceiveMessageAudio", System.Convert.ToBase64String(audioMemoryStream.ToArray()));

            return;
        }
    }
}
