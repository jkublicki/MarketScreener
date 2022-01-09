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
        //private const int timerIntervalMs = 5 * (1800 + 4000) + 2000; //HAPxYFManager.BatchSize * (HAPxYFManager.TickerSleepMs + 4000); //dodatkowo random i przerwa
        private const int timerIntervalMs = 5 * (1650 + 3000) + 2750; //22 s to chyba dolna granica; ok 1/3 przypadków to ticker skip; wydłużyć czas o 1/6, do 26 s
        private const int maxRunTimeH = 8; //8

        private static System.Timers.Timer triggerTimer;

        private static DateTime startTime;

        static void Main(string[] args)
        {
            //debug
            //Console.WriteLine(String.Join(";\n", new DataHunters.HAP.HapManager().Urls));
            //return;


            string line = "";

            SetTimer();

            Console.WriteLine("MarketScreener will be running for " + maxRunTimeH.ToString() + " hours from " + startTime.ToString("yyyy-MM-dd HH:mm") + " UTC, type STOP to break.");
            if (Log.DebugEnabled)
                Console.WriteLine("Debug enabled");

            if (Log.Enabled)
                Log.Entry("Application start");

            
            

            while (line != "STOP")
            {
                line = Console.ReadLine(); 
                
            }

            triggerTimer.Enabled = false;

            if (Log.Enabled)
                Log.Entry("Application stop");

        }

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            triggerTimer = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            triggerTimer.Elapsed += OnTimerTick;
            triggerTimer.AutoReset = true;
            triggerTimer.Enabled = true;

            startTime = DateTime.UtcNow;
        }

        private static void OnTimerTick(Object source, ElapsedEventArgs e)
        {
            if (HAPxYFManager.Status == HAPxYFManager.DataHunterStatus.OFF)
            {
                 if ((DateTime.UtcNow - startTime).TotalHours >= maxRunTimeH) //TotalHours
                {
                    if (Log.Enabled)
                        Log.Entry("Application stop");

                    Environment.Exit(0);
                }


                //zmiana interwału powoduje reset odliczania czasu
                triggerTimer.Enabled = false;
                triggerTimer.Interval = (int)Math.Floor(2000 * (decimal)(new Random().NextDouble() * 2.0 + 1)); //unikanie powtarzalności

                HAPxYFManager.Run();
                
                triggerTimer.Enabled = true;
            }
                
            else if (Log.Enabled)
                Log.Entry("Timer tick ignored, previous data hunter run has not finished.");
                
        }
    }
}
