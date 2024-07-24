using TwitchLib.Api;

namespace ChatBot
{
    public static class TwitchAPIFactory
    {
        private static TwitchAPI api = null;

        public static TwitchAPI GetAPI()
        {
            if (TwitchAPIFactory.api == null)
            {
                TwitchAPIFactory.api = new TwitchAPI();
                api.Settings.ClientId = Config.ClientID;
                api.Settings.Secret = Config.Secret; // App Secret is not an Accesstoken
            }

            return api;
        }
    }
}
