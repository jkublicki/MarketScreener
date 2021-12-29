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

            //test 
            //QueryDatabase.Test();

            Debug.WriteLine("wtf");

            //test HAPxYahooFinance
            List<string> r = DataHunters.HAPxYahooFinance.Test();
            foreach(string s in r)
            {
                Console.WriteLine(s);
            }
            //Console.ReadLine();

            Console.WriteLine("--- przed service");
            Console.WriteLine(DataHunters.HAPxYahooFinance.Service1());
            Console.WriteLine("--- po service");
        }
    }
}
