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
        const int batchSize = 2;

        public static DataHunterStatus Status;

        public enum DataHunterStatus
        {
            OFF,
            ON
        }

        private static List<string> GetYahooTickers()
        {
  
            List<string> tickers = new List<string>();

            //todo: uzupełnić słownik świąt, teraz są tylko weekendy

            //niepuste tickery yahoo, brak lub niedzisiejsza data aktualizacji, brak święta dzisiaj dla tego rynku
            string query = String.Concat("SELECT TOP ", batchSize.ToString(),
                " TickerYahoo FROM ENU_TICKER ET WHERE TickerYahoo IS NOT NULL ",
                "AND (UpdateDate IS NULL OR CAST(UpdateDate AS date) < CAST(GETDATE() AS date)) ",
                "AND NOT EXISTS (SELECT 1 FROM ENU_HOLIDAY EH WHERE EH.HolidayDate = CAST(GETDATE() AS date) ",
                "AND EH.MarketCodeGoogleFinance = LEFT(ET.TickerGoogleFinance, CHARINDEX(':', ET.TickerGoogleFinance) - 1))"
                );

            Console.WriteLine(query);

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

            List<string> tickers = GetYahooTickers();

            if (Log.Enabled)
                Log.Entry(String.Concat("Tickers: ", String.Join(", ", tickers)));

            string result = HAPxYahooFinance.Service(tickers);

            if (Log.Enabled)
                Log.Entry(result[..^1]);

            Status = DataHunterStatus.OFF;

            if (Log.Enabled)
                Log.Entry("HAPxYahooFinance end run");
        }

        

    }
}
