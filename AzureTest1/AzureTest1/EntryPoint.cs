using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MarketScreener.DataHunters.HAPxYahooFinance;

namespace MarketScreener
{
    internal class EntryPoint
    {
        private const int timerIntervalMs = 5 * (1800 + 4000) + 2000; //HAPxYFManager.BatchSize * (HAPxYFManager.TickerSleepMs + 4000); //dodatkowo random i przerwa
        private const int maxRunTimeH = 8;

        private static System.Timers.Timer triggerTimer;

        

        static void Main(string[] args)
        {
            string line = "";
            DateTime startTime = DateTime.Now;
            
            Console.WriteLine("MarketScreener will be running for " + maxRunTimeH.ToString() + " hours from " + startTime.ToString("yyyy-MM-dd HH:mm") + ", type STOP to break.");

            SetTimer();

            while (line != "STOP" || (DateTime.Now - startTime).Hours > maxRunTimeH)
            {
                line = Console.ReadLine();
            }


        }

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            triggerTimer = new System.Timers.Timer(timerIntervalMs);
            // Hook up the Elapsed event for the timer. 
            triggerTimer.Elapsed += OnTimerTick;
            triggerTimer.AutoReset = true;
            triggerTimer.Enabled = true;
        }

        private static void OnTimerTick(Object source, ElapsedEventArgs e)
        {
            if (HAPxYFManager.Status == HAPxYFManager.DataHunterStatus.OFF)
            {
                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " A KUKU!");

                HAPxYFManager.Run();
                
                triggerTimer.Interval = (int)Math.Floor(timerIntervalMs * (decimal)(new Random().NextDouble() * 0.05 + 1)); //unikanie powtarzalności
            }
                
            else if (Log.Enabled)
                Log.Entry("Timer tick ignored, previous data hunter till run has not finished.");
                
        }
    }
}
