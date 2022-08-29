using SteamKit2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

#nullable enable

namespace ChatBot.SteamWorks
{
    // Singleton. Provides hashed information about the device for auth attestation purposes.
    public class SysInfoProvider : IMachineInfoProvider
    {
        public static IMachineInfoProvider Instance
        {
            get
            {
                s_instance ??= new SysInfoProvider();
                return s_instance;
            }
        }

        private static SysInfoProvider? s_instance = null;

        private SysInfoProvider()
        {
            var di = DriveInfo.GetDrives().FirstOrDefault(d => d.DriveType == DriveType.Fixed && d.Name.ToLower() == @"c:\");
            _DiskId = Hash(string.Join('-', di?.VolumeLabel ?? "defaultVolume", di?.TotalSize.ToString() ?? "defaultSize", di?.DriveFormat ?? "defaultFormat"));

            _MacAddress = Hash(GetNetworkAdapters()
                .FirstOrDefault()?
                .GetPhysicalAddress()
                .GetAddressBytes());

            _MachineGuid = Hash(string.Join(".",
                string.IsNullOrEmpty(Environment.UserDomainName.Trim()) ? "UnknownDomain" : Environment.UserDomainName,
                string.IsNullOrEmpty(Environment.MachineName.Trim()) ? "UnknownMachineName" : Environment.MachineName,
                string.IsNullOrEmpty(Environment.UserName.Trim()) ? "UnknownUserName" : Environment.UserName));
        }


        byte[] IMachineInfoProvider.GetDiskId() => _DiskId;
        private readonly byte[] _DiskId;

        byte[] IMachineInfoProvider.GetMacAddress() => _MacAddress;
        private readonly byte[] _MacAddress;

        byte[] IMachineInfoProvider.GetMachineGuid() => _MachineGuid;
        private readonly byte[] _MachineGuid;


        private static byte[] Hash(byte[]? value)
        {
            const string salt = "-- Hello there. I hope you are doing well. This text is just hashing salt. Just salt.";

            using var sha = SHA1.Create();
            var b64val = value is null || value.Length < 1 ? "null null null" : Convert.ToBase64String(value);
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(b64val + salt));
            if (value != null && value.Length > 8 && value.Length <= hash.Length)
            {
                return hash.Take(value.Length).ToArray();
            }
            else
            {
                return hash;
            }
        }

        private static byte[] Hash(string? value)
        {
            return Hash(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static IEnumerable<NetworkInterface> GetNetworkAdapters()
        {
            // if you've got a DSL or some other wonky connection as the main one to your computer, adjust this boomer.
            // if you're switching primary connection methods a lot (wifi, ethernet, VPNs, tunnels, etc.), adjust this zoomer.
            var types = new List<NetworkInterfaceType>
                {
                    NetworkInterfaceType.Ethernet,
                    NetworkInterfaceType.Ethernet3Megabit,
                    NetworkInterfaceType.FastEthernetT,
                    NetworkInterfaceType.FastEthernetFx,
                    NetworkInterfaceType.Wireless80211,
                    NetworkInterfaceType.GigabitEthernet,
                    NetworkInterfaceType.Tunnel,
                    NetworkInterfaceType.Wman,
                    NetworkInterfaceType.Wwanpp,
                    NetworkInterfaceType.Wwanpp2
                };

            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var best = nics.Where(n
                    => n.OperationalStatus == OperationalStatus.Up
                    && !n.IsReceiveOnly
                    && types.Contains(n.NetworkInterfaceType))
                .OrderByDescending(n => n.Speed);

            return best;
        }
    }
}
