using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChatBot.Authentication
{
    public static class OAuth
    {
        public static CAccessRefreshTokens tokens = new CAccessRefreshTokens();
        public static ScopeCode scopeCode = new ScopeCode();
        public static string CodeURI = Config.RedirectURI + @"/Home/GetCode";
        public static string TokenURI = Config.RedirectURI + @"/Home/GetToken";

        public static async Task<string> GetBanAccessToken()
        {
            if (string.IsNullOrWhiteSpace(tokens.accessToken))
                await Populate();

            if (tokens.ExpiryDate < DateTime.Now)
                await Refresh();


            return tokens.accessToken;
        }

        private static async Task<bool> Populate()
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
              {
                  { "client_id", Config.ClientID },
                  { "client_secret", Config.Secret },
                  { "code", scopeCode.Code },
                  { "grant_type", "authorization_code" },
                  { "redirect_uri", TokenURI }
              };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // If the response contains a 401 then try refreshing
            if (responseString.Contains("401"))
                await Refresh();
            else if (responseString.Contains("access_token"))
                PopulateTokensFromJSON(responseString);
           

            return true;
        }

        private static async Task<bool> Refresh()
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
              {
                  { "client_id", Config.ClientID },
                  { "client_secret", Config.Secret },
                  { "grant_type", "refresh_token" },
                  { "refresh_token", HttpUtility.UrlEncode(tokens.refreshToken) }
              };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();

            PopulateTokensFromJSON(responseString);

            return true;
        }

        private static void PopulateTokensFromJSON(string response)
        {
            try
            {
                if (!response.Contains("access_token")) // Don't populate token data if the response doesn't contain a token
                    return;

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
                
                tokens.accessToken = result["access_token"];
                tokens.expiresIn = result["expires_in"];
                tokens.refreshToken = result["refresh_token"];
                tokens.scope = new List<string> { "ban" };
                tokens.tokenType = result["token_type"];
                tokens.ExpiryDate = DateTime.Now.AddSeconds(tokens.expiresIn); 
            }
            catch (Exception e)
            {
                SystemLogger.Log($"Failed to Populate OAuth tokens from JSON in OAuth.cs\n\n{e}");
            }
        }
    }
}
