taking all suggestions for new features :) 

currently in the process of trying to host this for free





















Documentation

Make a Config.cs file with all the missing variables that the IDE will tell you about


ChatBot/Properties/launchSettings:
{
  "profiles": {
    "ChatBot": {
      "commandName": "Project",
      "environmentVariables": {
        "GOOGLE_APPLICATION_CREDENTIALS ": "google how to use google voice authentication"
      }
    }
  }

YoutubeClient/Program.cs -> get ur own auth


Add ur google voice auth here
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:54549",
      "sslPort": 44381
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "KeyGOOGLE_APPLICATION_CREDENTIALS ": "C:\\Users\\Peer\\Desktop\\peerbot-329501-7bffcbd28a99.json",
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "YoutubeClient": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
