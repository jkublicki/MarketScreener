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
            SetTimer();

            if (Log.Enabled)
                Log.Entry("Application start");

            manager = new DataHunters.HAP.HAPManager();
            manager.Run();

            if (Log.Enabled)
                Log.Entry("Application stop");
        }

        private static void SetTimer()
        {
            triggerTimer = new System.Timers.Timer(1000);
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
