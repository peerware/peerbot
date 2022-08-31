using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;

#nullable enable

namespace ChatBot.SteamWorks
{
    public enum WatcherState
    {
        Invalid = 0,

        Connecting,
        Connected,
        LoggingIn,
        Received2FARequest,
        Submitting2FAResponse,
        LoggedIn,
        Searching,
        Idle,
        Disconnecting,

        Completed,
        Disconnected,
        LoginFailed,
        Errored,
        Disposed,
    }

    // Short-lived state if we get disconnected due to 2FA.
    public class Steam2FAData
    {
        public string? AppCode { get; set; } = null;
        public string? EmailCode { get; set; } = null;
    }

    public class LobbyWatcher : IDisposable
    {
        public Task? PollingTask { get; private set; } = null;

        public WatcherState Status { get; private set; } = WatcherState.Invalid;

        private readonly Steam2FAData _twoFAdata;
        private readonly List<IDisposable> _cbHandles;
        private readonly CallbackManager _cbManager;
        private readonly Action<IEnumerable<ThunderDomeLobby>> _matchAction;
        private readonly Func<ThunderDomeLobby, bool> _matchPredicate;
        private readonly SteamClient _steamClient;

        private uint? _cellId;

        private SteamMatchmaking MatcherImpl => _steamClient.GetHandler<SteamMatchmaking>()
            ?? throw new InvalidCastException("Failed to retrieve SteamMatchmaking interface");

        private SteamUser UserImpl => _steamClient.GetHandler<SteamUser>()
            ?? throw new InvalidCastException("Failed to retrieve SteamUser interface");

        private List<ThunderDomeLobby> _lobbies;

        public LobbyWatcher(Action<IEnumerable<ThunderDomeLobby>>? matchAction = null, Func<ThunderDomeLobby, bool>? matchPredicate = null)
        {
            if (matchAction is null)
            {
                _matchAction = lobbies =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var lobby in lobbies)
                    {
                        string state = lobby.NumPlayers >= 10 ? "nearly full" : lobby.NumPlayers >= 9 ? "filling up" : "seeding";
                        string branch = lobby.Branch == "public" ? string.Empty : $" (branch {lobby.Branch.ToUpper()})";
                        sb.Append($"{lobby.LobbyType} ThunderDome Lobby {lobby.Id % 1000}{branch} is {state} ({lobby.NumPlayers}/{lobby.MaxMembers}) with {lobby.AverageElo} elo.");
                    }
                    Console.WriteLine(sb.ToString());
                };
            }
            else
            {
                _matchAction = matchAction;
            }

            if (matchPredicate is null)
            {
                _matchPredicate = l =>
                {
                    return
                        (l.LobbyType == ELobbyType.Public || l.LobbyType == ELobbyType.FriendsOnly) &&
                        (l.Status == ELobbyState.Queueing) &&
                        (l.NumPlayers >= 8);
                };
            }
            else
            {
                _matchPredicate = matchPredicate;
            }

            _twoFAdata = new Steam2FAData();
            _steamClient = new SteamClient(SteamConfiguration.Create(c =>
            {
                c.WithCellID(GetCellId())
                 .WithMachineInfoProvider(SysInfoProvider.Instance)
                 .WithServerListProvider(new SteamKit2.Discovery.FileStorageServerListProvider(ServerListFilePath))
                 .WithUniverse(EUniverse.Public);
            }));
            _cbManager = new CallbackManager(_steamClient);
            _cbHandles = new List<IDisposable>
            {
                _cbManager.Subscribe<SteamClient.ConnectedCallback>(Client_Connected),
                _cbManager.Subscribe<SteamClient.DisconnectedCallback>(Client_Disconnected),
                _cbManager.Subscribe<SteamUser.LoginKeyCallback>(User_RefreshUserAuth),
                _cbManager.Subscribe<SteamUser.LoggedOnCallback>(User_LoggedIn),
                _cbManager.Subscribe<SteamUser.LoggedOffCallback>(User_LoggedOut),
                _cbManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(User_RefreshMachineAuth),
                _cbManager.Subscribe<SteamMatchmaking.GetLobbyListCallback>(MatchMaking_ReceivedLobbies)
            };

            if (!Directory.Exists(Path.Join(Config.fileSavePath, "steam")))
            {
                Directory.CreateDirectory(Path.Join(Config.fileSavePath, "steam"));
            }
        }

        public void Dispose()
        {
            if (_steamClient.IsConnected)
            {
                _steamClient.Disconnect();
                _cbManager.RunWaitAllCallbacks(TimeSpan.FromSeconds(5));
            }

            if (_cbHandles.Count > 0)
            {
                foreach (var cbHandle in _cbHandles)
                {
                    cbHandle.Dispose();
                }
                _cbHandles.Clear();
            }

            PollingTask?.Dispose();
            PollingTask = null;
        }

        // Called in program.cs - the entry into lobby watcher
        public void BeginPolling()
        {
            // Create the polling task
            PollingTask = Task.Factory.StartNew(() =>
            {
                // Connect the steam client and update the LobbyWatcher status
                _steamClient.Connect();
                Status = WatcherState.Connecting;

                // Run this while the status of the watcher is connected(ish)
                while ( Status >= WatcherState.Connecting && Status <= WatcherState.Disconnecting)
                {
                    _cbManager.RunWaitAllCallbacks(TimeSpan.FromMilliseconds(1000));
                }
                Status = WatcherState.Completed;
            });
        }


        void Client_Connected(SteamClient.ConnectedCallback cb)
        {
            Status = WatcherState.Connected;

            // TODO: check out SteamUser.LogOnAnonymous, as we might able to do this without auth...
            UserImpl.LogOn(new SteamUser.LogOnDetails
            {
                Username = Config.SteamUsername,

                // TODO: these are really only needed during first authentication, but it seems
                // silly to read the password from the console given the app's current design.
                Password = Config.SteamPassword,

                // 2fa tokens *will* be read from the console.
                AuthCode = _twoFAdata.EmailCode,
                TwoFactorCode = _twoFAdata.AppCode,

                LoginKey = GetUserKey(),
                ShouldRememberPassword = true,

                SentryFileHash = GetMachineKey(),

                // set this value non-null to not interfere with the actual Steam client that's likely also running on this computer.
                LoginID = 6942069,
            });
            Status = WatcherState.LoggingIn;

            _twoFAdata.AppCode = null;
            _twoFAdata.EmailCode = null;
        }

        void Client_Disconnected(SteamClient.DisconnectedCallback cb)
        {
            // 2FA requests will knock us offline during the auth procedure, so just ignore it.
            if (!(Status == WatcherState.Received2FARequest || Status == WatcherState.Submitting2FAResponse))
            {
                Status = WatcherState.Disconnected;
                Console.WriteLine("Disconnected from Steam");
            }
        }

        void User_LoggedIn(SteamUser.LoggedOnCallback cb)
        {
            if (cb.Result == EResult.AccountLoginDeniedNeedTwoFactor || cb.Result == EResult.AccountLogonDenied)
            {
                Status = WatcherState.Received2FARequest;

                Console.Write($"Please enter the 2FA code from your {(cb.Result == EResult.AccountLoginDeniedNeedTwoFactor ? "authenticator app" : "email")}: ");

                if (cb.Result == EResult.AccountLoginDeniedNeedTwoFactor)
                {
                    _twoFAdata.AppCode = Console.ReadLine();
                }
                else if (cb.Result == EResult.AccountLogonDenied)
                {
                    _twoFAdata.EmailCode = Console.ReadLine();
                }

                _cbManager.RunWaitCallbacks(TimeSpan.FromSeconds(2));
                Thread.Sleep(2000);
                _steamClient.Connect();

                Status = WatcherState.Submitting2FAResponse;
                _cbManager.RunWaitCallbacks();

                return;
            }
            else if (cb.Result != EResult.OK)
            {
                Console.WriteLine($"Unable to login to Steam: {cb.Result} / {cb.ExtendedResult}.");
                Status = WatcherState.LoginFailed;

                return;
            }

            // If neither of the above applied, we're now logged in to steam.
            Status = WatcherState.LoggedIn;
            if (cb.CellID != _cellId)
            {
                _cellId = cb.CellID;
                SetCellId(cb.CellID);
            }

            Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    Thread.Sleep(10 * 1000);
                    MatcherImpl.GetLobbyList(Ns2AppId);
                    Status = WatcherState.Searching;
                }
            });
        }

        void User_LoggedOut(SteamUser.LoggedOffCallback cb)
        {
            _steamClient.Disconnect();

            // Delete the client token. I don't actually invoke logging out anywhere, but this token's now entirely pointless.
            if (File.Exists(UserKeyFilePath))
            {
                try
                {
                    File.Delete(UserKeyFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete Steam auth token file \"{UserKeyFileName}\": {ex.Message}");
                }
            }
        }

        async void User_RefreshUserAuth(SteamUser.LoginKeyCallback cb)
        {
            await File.WriteAllTextAsync(UserKeyFilePath, cb.LoginKey)
                .ContinueWith((t) =>
                {
                    if (!t.IsFaulted && t.IsCompleted)
                        UserImpl.AcceptNewLoginKey(cb);
                });

        }

        async void User_RefreshMachineAuth(SteamUser.UpdateMachineAuthCallback cb)
        {
            bool err = false;
            int fileSize = 0;
            try
            {
                using (var fs = File.Open(MachineKeyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    fs.Seek(cb.Offset, SeekOrigin.Begin);
                    await fs.WriteAsync(cb.Data, 0, cb.Data.Length);
                    fileSize = (int)fs.Length;
                }
            }
            catch
            {
                err = true;
            }

            UserImpl.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = cb.JobID,
                FileName = cb.FileName,
                FileSize = fileSize,
                Offset = cb.Offset,
                Result = err ? EResult.UnexpectedError : EResult.OK,
                LastError = 0,
                OneTimePassword = cb.OneTimePassword,
                SentryFileHash = GetMachineKey(),
            });
        }

        void MatchMaking_ReceivedLobbies(SteamMatchmaking.GetLobbyListCallback cb)
        {
            var lobbies = cb.Lobbies
                .Select(l => ThunderDomeLobby.FromSteamKit(l))
                .Where(l => _matchPredicate(l))
                .ToList();

            // Only perform the predicate matched action when a unique lobby is found (unqiue from its ID)
            foreach (var lobby in lobbies)
            {
                _lobbies
            }

            if (lobbies.Any())
            {
                _matchAction(lobbies);
            }
        }


        private static uint GetCellId()
        {
            try
            {
                if (File.Exists(CellIdFilePath) && uint.TryParse(File.ReadAllText(CellIdFilePath), out var cellId))
                {
                    return cellId;
                }
            }
            catch
            {
            }
            return 0;
        }

        private static void SetCellId(uint value)
        {
            File.WriteAllText(CellIdFilePath, value.ToString());
        }

        private static byte[]? GetMachineKey()
        {
            try
            {
                if (File.Exists(MachineKeyFilePath))
                {
                    using var fs = File.Open(MachineKeyFilePath, FileMode.Open, FileAccess.Read);
                    using var sha = SHA1.Create();
                    return sha.ComputeHash(fs);
                }
            }
            catch
            {
            }
            return null;
        }

        private static string? GetUserKey()
        {
            try
            {
                if (File.Exists(UserKeyFilePath))
                {
                    return File.ReadAllText(UserKeyFilePath);
                }
            }
            catch
            {
            }
            return null;
        }


        // Steam Works wants to save a lot of stuff to disk. Server lists, auth tokens, etc.
        // All of it is essentially junk, so keep it confined to %LocalAppData%\PeerBot\steam\
        private static string GetConfigFilePath(string fileName)
        {
            // TODO: if desired: string datadir = Config.fileSavePath;
            string datadir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PeerBot");

            if (!Directory.Exists(datadir))
            {
                Directory.CreateDirectory(datadir);
            }

            datadir = Path.Combine(datadir, "steam");
            if (!Directory.Exists(datadir))
            {
                Directory.CreateDirectory(datadir);
            }

            return Path.Join(datadir, fileName);
        }


        private const int Ns2AppId = 4920;

        // Contains a number that represents your geographic region to steam.
        private const string CellIdFileName = "cellid.txt";
        private static string CellIdFilePath { get; } = GetConfigFilePath(CellIdFileName);

        // Contains machine auth token. *** File is controlled entirely by Valve through callbacks ***
        // is hashed and sent to attest during user authentication.
        private const string MachineKeyFileName = "machine.bin";
        private static string MachineKeyFilePath { get; } = GetConfigFilePath(MachineKeyFileName);

        // Contains user's auth token.
        private const string UserKeyFileName = "userid.txt";
        private static string UserKeyFilePath { get; } = GetConfigFilePath(UserKeyFileName);

        // Contains information about steam servers.
        private const string ServerListFileName = "servers.bin";
        private static string ServerListFilePath { get; } = GetConfigFilePath(ServerListFileName);
    }
}
