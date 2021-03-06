using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace MarketScreener.Test
{
    internal static class SelfTest
    {
        public static string RunSelfTest(out bool success)
        {
            string report = "";
            success = true;

            if (Tools.Levenshtein.LevenshteinDistance("pies", "pies") != 0)
            {
                report += "Problem with Tools.Levenshtein.LevenshteinDistance()\n";
                success = false;
            }

            if (GICS.GetGICSCodeAndCategory("ENERGY").Item1 != 10)
            {
                report += "Problem with GICS.GetGICSCodeAndCategory()\n";
                success = false;
            }

            if (QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, "SELECT TOP 1 TickerGF FROM ENU_TICKER", false, out DataTable _) == -1)
            {
                report += "Problem with QueryDatabase.ExecuteSQLStatement()\n";
                success = false;
            }

            if (DataHunters.HAP.StringConverters.ConvertValue("1.23 - 1.34", DataHunters.HAP.StringConverters.ConvertingFunctions.DecimalRangeLeft, null, out bool _) != "1.23")
            {                
                report += "Problem with DataHunters.HAPxYahooFinance.NodeConverters.ConvertValue()\n";
                success = false;
            }

    
            return report;
        }
    }
}
