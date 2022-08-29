using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2;

#nullable enable

namespace ChatBot.SteamWorks
{
    public enum ELobbyState
    {
        Unknown = 0,
        Queueing = 1,
        Unknown2 = 2, // Choosing commanders?
        Unknown3 = 3, // Choosing maps?
        Unknown4 = 4, // Choosing life forms (300s spinup)
        Unknown5 = 5, // Playing first round?
        Unknown6 = 6, // Playing second round?
        Completed = 7,
    }

    /// <summary>A data class containing information about a ThunderDome lobby.</summary>
    public class ThunderDomeLobby
        : IComparable<ThunderDomeLobby>
        , IEquatable<ThunderDomeLobby>
    {
        public ThunderDomeLobby()
        {
        }


        /// <summary>Gets or sets the median skill of the lobby.</summary>
        public int AverageElo { get; set; } = 0;

        /// <summary>Gets or sets the branch of the game the lobby is operating on.</summary>
        /// <remarks>At least "public" and "beta" are valid options, but there's presumably others.</remarks>
        public string Branch { get; set; } = "public";

        /// <summary>Gets or sets the build number of the game the lobby is operating on.</summary>
        /// <remarks>This is currently at <c>341</c> at the time of this writing.</remarks>
        public int Build { get; set; } = 0;

        /// <summary>Gets or sets the straight-line distance to the median lobby location.</summary>
        /// <remarks>The meaning of this value has not been fully fleshed out. It is quite likely that
        /// this value is just "miles from your location to the average player location", and the
        /// closest datacenter to this location will be used to spin up the game server.</remarks>
        public float Distance { get; set; } = 0f; // TODO: Is this miles? Kilometers? Locale-aware? Reasonably accurate?

        /// <summary>Gets or sets the unique identifier of the lobby.</summary>
        public ulong Id { get; set; } = 0;

        /// <summary>Gets or sets the type of the lobby, such as Private, or Invisible.</summary>
        public ELobbyType LobbyType { get; set; } = ELobbyType.Private;

        /// <summary>Gets or sets the total number of players that could join the lobby, but generally
        /// this value should be 12.</summary>
        public int MaxMembers { get; set; } = 12;

        /// <summary>Gets or sets the number of players that have joined the lobby.</summary>
        public int NumPlayers { get; set; } = 0;

        /// <summary>Gets or sets the status of the lobby.</summary>
        public ELobbyState Status { get; set; } = ELobbyState.Unknown;



        public int CompareTo(ThunderDomeLobby? other)
        {
            if (other is null)
                return 1;
            return Id.CompareTo(other.Id);
        }

        public bool Equals(ThunderDomeLobby? other)
        {
            return other != null && Id == other.Id;
        }


        internal static ThunderDomeLobby FromSteamKit(SteamMatchmaking.Lobby lobby)
        {
            return new ThunderDomeLobby
            {
                AverageElo = lobby.Metadata.TryGetValue("MedianSkill", out var skillValueStr) && int.TryParse(skillValueStr, out int skillValue)
                    ? skillValue
                    : 0,
                Branch = lobby.Metadata.TryGetValue("Branch", out var branchValueStr) && !string.IsNullOrWhiteSpace(branchValueStr)
                    ? branchValueStr
                    : "public",
                Build = lobby.Metadata.TryGetValue("Build", out var buildValueStr) && int.TryParse(buildValueStr, out int buildValue)
                    ? buildValue
                    : 0,
                Distance = lobby.Distance ?? 0,
                Id = lobby.SteamID.ConvertToUInt64(),
                LobbyType = lobby.LobbyType,
                MaxMembers = lobby.MaxMembers,
                NumPlayers = lobby.NumMembers,
                Status = lobby.Metadata.TryGetValue("State", out var stateValueStr) && int.TryParse(stateValueStr, out int stateValue)
                    ? (ELobbyState)stateValue
                    : ELobbyState.Unknown
            };
        }
    }
}
