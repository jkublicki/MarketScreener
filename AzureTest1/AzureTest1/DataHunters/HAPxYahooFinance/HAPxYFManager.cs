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

            string query = String.Concat("SELECT TOP ", BatchSize.ToString(), " TickerYF ",
                "FROM ENU_TICKER ET JOIN ENU_MARKET EM ON ET.MarketCodeGF = EM.MarketCodeGF ",
                "LEFT JOIN ENU_HOLIDAY EH ON EM.MarketCodeGF = EH.MarketCodeGF AND EH.HolidayDay = CAST(GETUTCDATE() AS date) ",
                "WHERE ET.TickerYF IS NOT NULL AND (ET.UpdateDate IS NULL OR CAST(ET.UpdateDate AS date) < CAST(GETUTCDATE() AS date)) ",
                "AND EH.HolidayDay IS NULL ");

            if (skipOpenMarkets)
            {
                string utcNow = ((decimal)(DateTime.UtcNow.Hour + ((decimal)DateTime.UtcNow.Minute / (decimal)100.0))).ToString(System.Globalization.CultureInfo.InvariantCulture);
                query = String.Concat(query, "AND EM.OpenHourUTC IS NOT NULL AND EM.CloseHourUTC IS NOT NULL ",
                "AND (", utcNow, " < EM.OpenHourUTC OR ", utcNow, " > EM.CloseHourUTC) ",
                "ORDER BY EM.MinComissionEUR ASC, ET.MarketCapMnUSD DESC");
            }
            else
            {
                query = String.Concat(query, "ORDER BY EM.MinComissionEUR ASC, ET.MarketCapMnUSD DESC");
            }

            int rows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable? dataTable);

            if (rows > 0 && dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (!row.IsNull(0) && row.ItemArray.Length > 0 && row.ItemArray[0] != null)
                        tickers.Add(row.ItemArray[0].ToString()); //warning VS bez sensu
                }
            }

            return tickers;
        }

        public static void Run()
        {
            if (Log.Enabled)
                Log.Entry("HAPxYahooFinance start run");
            
            Status = DataHunterStatus.ON;

            List<string> tickers = GetYahooTickers(false); //powinno być ignore open markets = true

                    //chamski TEST-usunąć!!!                    
                    //tickers.Clear();
                    //tickers.Add("ATD.WA");
                    

            if (Log.Enabled)
                Log.Entry(String.Concat("Tickers: ", String.Join(", ", tickers)));

            //HAPxYahooFinance.Service(tickers, TickerSleepMs);
            new HAP.HAPDataExtractor().Extract(@"BAVA.CO", @"https://finance.yahoo.com/quote/BAVA.CO", new HAP.WebsiteStructure("CRAWL_1"));


            Status = DataHunterStatus.OFF;

            if (Log.Enabled)
                Log.Entry("HAPxYahooFinance end run");
        }

        

    }
}
