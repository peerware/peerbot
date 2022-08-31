using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ChatBot.SteamWorks;

namespace ChatBot
{
    class Program
    {
        public static void Main()
        {
            // Monitor thunderdome lobbies. Info will go to the console.
            //using (var cts = new CancellationTokenSource())
            //using (var tdwatcher = new LobbyWatcher(ReceivedLobbies,
            //    l => l.AverageElo >= 100 && l.NumPlayers >= 1 && l.NumPlayers < l.MaxMembers && l.Status == ELobbyState.Queueing))
            //{
            //    tdwatcher.BeginPolling(cts.Token);

            //    Console.WriteLine("Press 'Q' to end program ..." + Environment.NewLine);
            //    while (Console.ReadKey(true).Key != ConsoleKey.Q && tdwatcher.PollingTask.Status == System.Threading.Tasks.TaskStatus.Running)
            //    {
            //        Thread.Sleep(200);
            //    }
            //    cts.Cancel();

            //    if (!tdwatcher.PollingTask.Wait(10 * 1000))
            //    {
            //        Console.WriteLine("took longer than 10 seconds to shutdown. naughty boi.");
            //    }
            //}
        }
    }
}
