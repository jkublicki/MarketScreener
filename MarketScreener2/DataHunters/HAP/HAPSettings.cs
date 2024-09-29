using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal static class HAPSettings
    {        
        public const bool LogEnabled = true; //default: true
        public const double DelayBase = 3093; //default: 3000, it's important; wait time ms = R > LongDelayChance ? DelayBase * (R1 * DelayRandomMul  + 1) : DelayBase * (R1 * DelayRandomMul  + LongDelayRandomMod)
        public const double DelayRandomMul = 2; //default: 2
        public const double LongDelayChance = 0.15; //default: 0.15
        public const int LongDelayRandomMod = 20; //default 20; 1 will be used in case short delay

            //!! odkręcić 
        public static bool DebugEnabled = true; //default: false; Save doc, detailed log, overwrite url (if not null) //PRZENIEŚĆ DO BAZY!!!!!
        //public const bool SaveBrokenWebsites = false; //default: false
        public const bool SkipDataExtraction = false; //default: false
        public static (string, string)? TestUrl = null;// ("WMT", "https://finance.yahoo.com/quote/WMT") ; //ie. ("BAC", "https://finance.yahoo.com/quote/bac"); default: null; Overwrite url set (if DebugEnabled)

        public static string Print()
        {
            return String.Concat("LogEnabled: ", LogEnabled.ToString(),
                "\nDelayBase: ", DelayBase,
                "\nDelayRandomMul: ", DelayRandomMul,
                "\nLongDelayChance: ", LongDelayChance,
                "\nLongDelayRandomMod: ", LongDelayRandomMod,
                //"\nSaveBrokenWebsites: ", SaveBrokenWebsites,
                "\nSkipDataExtraction: ", SkipDataExtraction,
                "\nDebugEnabled: ", DebugEnabled ? "True (save docs, detailed log, overwrite url set if test url is not null)" : "False",
                "\nTestUrl: ", TestUrl.HasValue ? (TestUrl.Value.Item1 + ", " + TestUrl.Value.Item2 + " (works only with DebugEnabled = True)") : "N/A", "\n"
                );
        }
        
        
    }
}
