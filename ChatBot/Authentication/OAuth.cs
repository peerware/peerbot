using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace ChatBot.Authentication
{
    public static class OAuth
    {
        public static string CodeURI = Config.RedirectURI + @"/Home/GetCode";
        public static string TokenURI = Config.RedirectURI + @"/Home/GetToken";

        public static Dictionary<eScope, CAccessRefreshTokens> accessTokens = new Dictionary<eScope, CAccessRefreshTokens>();

        public enum eScope
        {
            ban,
            manageChannel,
            clips
        }

        public static string GetScope(eScope scope)
        {
            switch (scope)
            {
                case eScope.ban:
                    return "moderator:manage:banned_users";
                case eScope.manageChannel:
                    return "channel:manage:broadcast";
                case eScope.clips:
                    return "clips:edit";
            }

            return "";
        }

        public static eScope GetScope(string scope)
        {
            switch (scope)
            {
                case "moderator:manage:banned_users":
                    return eScope.ban;
                case "channel:manage:broadcast":
                    return eScope.manageChannel;
                case "clips:edit":
                    return eScope.clips;
            }

            return eScope.ban;
        }

        public static async Task<string> GetAccessToken(eScope scope)
        {
            // If the dictionary doesn't have the key then we can't authenticate as we don't have a code
            if (!accessTokens.ContainsKey(scope))
                return "";

            if (string.IsNullOrEmpty(accessTokens[scope].accessToken))
                await Populate(scope);

            if (accessTokens[scope].ExpiryDate < DateTime.Now)
                await Refresh(scope);

            return accessTokens[scope].accessToken;
        }

        private static async Task<bool> Populate(eScope scope)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
              {
                  { "client_id", Config.ClientID },
                  { "client_secret", Config.Secret },
                  { "code", accessTokens[scope].code },
                  { "grant_type", "authorization_code" },
                  { "redirect_uri", TokenURI }
              };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // If the response contains a 401 then try refreshing
            if (responseString.Contains("401"))
                await Refresh(scope);
            else if (responseString.Contains("access_token"))
                PopulateTokensFromJSON(responseString, scope);
           

            return true;
        }

        private static async Task<bool> Refresh(eScope scope)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
              {
                  { "client_id", Config.ClientID },
                  { "client_secret", Config.Secret },
                  { "grant_type", "refresh_token" },
                  { "refresh_token", HttpUtility.UrlEncode(accessTokens[scope].refreshToken) }
              };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();

            PopulateTokensFromJSON(responseString, scope);

            return true;
        }

        private static void PopulateTokensFromJSON(string response, eScope scope)
        {
            try
            {
                if (!response.Contains("access_token")) // Don't populate token data if the response doesn't contain a token
                    return;

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
                
                accessTokens[scope].accessToken = result["access_token"];
                accessTokens[scope].expiresIn = result["expires_in"];
                accessTokens[scope].refreshToken = result["refresh_token"];
               // scopeTokens[scope]scope = new List<string> { "ban" };
                accessTokens[scope].tokenType = result["token_type"];
                accessTokens[scope].ExpiryDate = DateTime.Now.AddSeconds(accessTokens[scope].expiresIn); 
           
            }
            catch (Exception e)
            {
                SystemLogger.Log($"Failed to Populate OAuth tokens from JSON in OAuth.cs\n\n{e}");
            }
        }
    }
}
