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
            Console.WriteLine("Start");
            Console.WriteLine("QueryDatabase.Test():");
            QueryDatabase.Test();

            Console.WriteLine("HAPxYahooFinance.Test():");
            List<string> r = DataHunters.HAPxYahooFinance.HAPxYahooFinance.Test();
            foreach(string s in r)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine("Przed DataHunters.HAPxYahooFinance.HAPxYahooFinance.Service()\n");
            Console.WriteLine(DataHunters.HAPxYahooFinance.HAPxYahooFinance.Service());
            Console.WriteLine("\nPo DataHunters.HAPxYahooFinance.HAPxYahooFinance.Service()");
        }
    }
}
