using System;
using static ChatBot.Authentication.OAuth;

namespace ChatBot.Authentication
{
    public class CAccessRefreshTokens
    {
        public DateTime ExpiryDate;
        public string accessToken;
        public string refreshToken; // Used to refresh the access token
        public int expiresIn;
        public string tokenType;
        public eScope scope;
        public string code; // Used to get the initial access token
    }
}
