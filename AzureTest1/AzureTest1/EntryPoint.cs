using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MarketScreener
{
    internal class EntryPoint
    {
        private static System.Timers.Timer triggerTimer;
        private static DataHunters.HAP.HAPManager manager;

        static void Main(string[] args)
        {
            List<string> longArgs = new();
            List<char> shortArgs = new();
            foreach (string a in args)
            {
                if (a.Length > 3 && String.Equals(a.Substring(0, 2), "--"))
                    longArgs.Add(a.Substring(2, a.Length - 2));
                else if (a.Length == 2 && a[0] == '-' && a[1] != '-')
                    shortArgs.Add(a[1]);
            }

            SetTimer();

            if (DataHunters.HAP.HAPSettings.LogEnabled)
                Log.Entry("Application start");

            manager = new DataHunters.HAP.HAPManager();

            if (longArgs.Count > 0)
                manager.Run(longArgs.First());
            else
                manager.Run(null);

            if (DataHunters.HAP.HAPSettings.LogEnabled)
                Log.Entry("Application stop");
        }

        private static void SetTimer()
        {
            triggerTimer = new System.Timers.Timer(500);
            triggerTimer.Elapsed += OnTimerTick;
            triggerTimer.AutoReset = true;
            triggerTimer.Enabled = true;
        }

        private static void OnTimerTick(Object source, ElapsedEventArgs e)
        {
            string line = Console.ReadLine();
            if (line == "STOP")
                manager.BreakRun();            
        }

    }
}
