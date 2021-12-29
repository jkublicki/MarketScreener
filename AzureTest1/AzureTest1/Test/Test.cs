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
            //Console.WriteLine((DataHunters.HAPxYahooFinance.NodeConverters.DecimalRangeRight("123.45 - 123.99", out _) + 1).ToString());



            //test 

            Console.WriteLine("Start");

            QueryDatabase.Test();

            Debug.WriteLine("wtf");

            //test HAPxYahooFinance
            List<string> r = DataHunters.HAPxYahooFinance.HAPxYahooFinance.Test();
            foreach(string s in r)
            {
                Console.WriteLine(s);
            }
            //Console.ReadLine();

            Console.WriteLine("--- przed service");
            Console.WriteLine(DataHunters.HAPxYahooFinance.HAPxYahooFinance.Service1a());
            Console.WriteLine("--- po service");
        }
    }
}
