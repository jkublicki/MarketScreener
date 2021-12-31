using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal static class HAPxYFManager
    {
        //jak spowodowac zeby program sie nie ztrzymywal?
        //ani nie zarl duzo zasobow
        //jakies dobre praktyki do uslug


        const int interval = 3000; //ms

        static System.Timers.Timer timer;

        public static void StartTimer()
        {
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public static void StopTimer()
        {
            timer.Dispose();
        }


        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                              e.SignalTime);
        }

    }
}
