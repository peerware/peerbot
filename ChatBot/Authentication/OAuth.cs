using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Authentication
{
    public static class OAuth
    {
        public static CAccessRefreshTokens tokens = new CAccessRefreshTokens();
        public static ScopeCode scopeCode = new ScopeCode();
        public static string CodeURI = @"https://localhost:44381/Home/GetCode";
        public static string TokenURI = @"https://localhost:44381/Home/GetToken";

        public static async Task<string> GetBanAccessToken()
        {
            if (string.IsNullOrWhiteSpace(tokens.accessToken))
                await Populate();

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
             
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString);

            tokens.accessToken = result["access_token"];
            tokens.expiresIn = result["expires_in"];
            tokens.refreshToken = result["refesh_token"];
            tokens.scope = new List<string> { "ban" };
            tokens.tokenType = result["token_type"];

            return true;
        }
    }
}
