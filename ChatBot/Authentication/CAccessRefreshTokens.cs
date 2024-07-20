using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Authentication
{
    public class CAccessRefreshTokens
    {
        public DateTime ExpiryDate;
        public string accessToken;
        public string refreshToken;
        public int expiresIn;
        public List<string> scope;
        public string tokenType;
    }
}
