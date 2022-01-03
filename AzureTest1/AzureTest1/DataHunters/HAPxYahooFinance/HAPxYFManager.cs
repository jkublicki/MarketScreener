using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Data;

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal static class HAPxYFManager
    {
        public const int BatchSize = 5;
        public const int TickerSleepMs = 1500;

        public static DataHunterStatus Status;

        public enum DataHunterStatus
        {
            OFF,
            ON
        }

        private static List<string> GetYahooTickers(bool skipOpenMarkets)
        {
  
            List<string> tickers = new List<string>();

            //todo: uzupełnić słownik świąt, teraz są tylko weekendy

            string query = String.Concat("SELECT TOP ", BatchSize.ToString(), " TickerYahoo ",
                "FROM ENU_TICKER ET JOIN ENU_MARKET EM ON ET.MarketCodeGoogleFinance = EM.MarketCodeGoogleFinance ",
                "LEFT JOIN ENU_HOLIDAY EH ON EM.MarketCodeGoogleFinance = EH.MarketCodeGoogleFinance AND EH.HolidayDate = CAST(GETUTCDATE() AS date) ",
                "WHERE ET.TickerYahoo IS NOT NULL AND (ET.UpdateDate IS NULL OR CAST(ET.UpdateDate AS date) < CAST(GETUTCDATE() AS date)) ",
                "AND EH.HolidayDate IS NULL ");

            if (skipOpenMarkets)
            {
                string utcNow = ((decimal)(DateTime.UtcNow.Hour + ((decimal)DateTime.UtcNow.Minute / (decimal)100.0))).ToString(System.Globalization.CultureInfo.InvariantCulture);
                query += String.Concat("AND EM.OpenHourUTC IS NOT NULL AND EM.CloseHourUTC IS NOT NULL ",
                "AND (", utcNow, " < EM.OpenHourUTC OR ", utcNow, " > EM.CloseHourUTC) ",
                "ORDER BY EM.MinComissionEUR ASC, ET.MarketCapMnUSD DESC");
            }

            //Console.WriteLine(query);

            int rows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable dataTable);

            if (rows > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row != null && !row.IsNull(0))
                        tickers.Add(row.ItemArray[0].ToString());
                }
            }

            return tickers;
        }

        public static void Run()
        {
            if (Log.Enabled)
                Log.Entry("HAPxYahooFinance start run");
            
            Status = DataHunterStatus.ON;

            List<string> tickers = GetYahooTickers(true);

                    //TEST-usunąć!!!
                    /*
                    tickers.Clear();
                    tickers.Add("YNDX");
                    */

            if (Log.Enabled)
                Log.Entry(String.Concat("Tickers: ", String.Join(", ", tickers)));                

            string result = HAPxYahooFinance.Service(tickers, TickerSleepMs);

            if (Log.Enabled)
                Log.Entry(result[..^1]);

            Status = DataHunterStatus.OFF;

            if (Log.Enabled)
                Log.Entry("HAPxYahooFinance end run");
        }

        

    }
}
