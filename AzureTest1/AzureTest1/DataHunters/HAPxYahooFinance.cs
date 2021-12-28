using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;


namespace MarketScreener.DataHunters
{
    internal static class HAPxYahooFinance
    {
        private static List<string> GetYahooTickers()
        {
            List<string> tickers = new List<string>();
            string query = "SELECT TickerYahoo FROM ENU_TICKER WHERE TickerYahoo IS NOT NULL";            
            int rows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable dataTable);

            
            if (rows > 0)
            {
                foreach(DataRow row in dataTable.Rows)
                {
                    if (row != null && !row.IsNull(0))
                        tickers.Add(row.ItemArray[0].ToString());
                }
            }

            return tickers;
        }

        public static void Service()
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            const string searchElementPriceBeg1 = "data-field=\"regularMarketPrice\"";
            const string searchElementPriceBeg2 = ">";
            const string searchElementPriceEnd = "<";

            List<string> tickers = GetYahooTickers();
            

            var web = new HtmlAgilityPack.HtmlWeb();

            Debug.WriteLine("HELLO");

            if (tickers.Count > 0)
            {
                foreach(string t in tickers)
                {
                    string url = urlBase + t;
                    var doc = web.Load(url);
                    string docText = doc.Text;

                    int idxBeg = docText.IndexOf(searchElementPriceBeg1);
                    idxBeg = docText.IndexOf(searchElementPriceBeg2, idxBeg);
                    int idxEnd = docText.IndexOf(searchElementPriceEnd, idxBeg);

                    Debug.WriteLine(docText.Substring(idxBeg, idxEnd));

                    /*
                     * todo:
                     * opanować Debug.WriteLine https://stackoverflow.com/questions/9466838/writing-to-output-window-of-visual-studio
                     * sprawdzić wyłuskiwanie danych ze strony yahoo finance
                     * dodać zapis do bazy
                     * dodać generalnie obsługę wielu punktów danych, w tym celu ich search elements itp.
                     * na teraz zrobić to w jednej długiej funkcji
                     * zweryfikowac ze zawsze search elements działają np. yahoo z tym nie walczy różnymi wersjami strony
                     * zweryfikować że program się wyrabia i nie zakleszcza
                     * 
                     */
                }
            }
        }

        public static List<string> Test()
        {
            List<string> yt = GetYahooTickers();

            if (yt.Count > 0)
                return yt;
            else
                throw new Exception("Błąd w DataHunters.GetYahooTickers()");

        }
    }
}
