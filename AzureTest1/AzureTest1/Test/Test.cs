using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MarketScreener.Test
{
    internal class Test
    {
        static void Main(string[] args)
        {

            

            //nie kasować testów, potem zrobić metodę diagnostyczną i porównanie wyników

            //Console.WriteLine(GICS.GetGICSCodeAndCategory("TECHNOLOGY").Item1.ToString());

            //DataHunters.HAPxYahooFinance.HAPxYFManager.StartTimer();

            //Console.ReadLine();


            //Console.WriteLine("Start");
            //Console.WriteLine("QueryDatabase.Test():");
            //QueryDatabase.Test();

            /*
            Console.WriteLine("HAPxYahooFinance.Test():");
            List<string> r = DataHunters.HAPxYahooFinance.HAPxYahooFinance.Test();
            foreach(string s in r)
            {
                Console.WriteLine(s);
            }
            */







            DataHunters.HAPxYahooFinance.HAPxYFManager.Run();

            
            
            
        }
    }
}
