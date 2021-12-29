using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;


/*
 * todo:
 * teraz: https://html-agility-pack.net/select-nodes
 * - bo se nie mają przyszłości i zaczęły powodować problemy
 * 
 * 
 * opanować Debug.WriteLine https://stackoverflow.com/questions/9466838/writing-to-output-window-of-visual-studio
 * opanować debugowanie (breakpoint, watch), coś jest nie tak
 * sprawdzić wyłuskiwanie danych ze strony yahoo finance
 * dodać zapis do bazy
 * dodać generalnie obsługę wielu punktów danych, w tym celu ich search elements itp.
 * na teraz zrobić to w jednej długiej funkcji
 * zweryfikowac ze zawsze search elements działają np. yahoo z tym nie walczy różnymi wersjami strony
 * zweryfikować że program się wyrabia i nie zakleszcza
 * potem pomyśleć o bardziej inteligentnej obsłudze
 * 
 * korzystając z DocumentNode.SelectNodes w chrome pobierać full XPATH
 * 
 */

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

        public static string Service1()
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            List<string> tickers = GetYahooTickers();
            var web = new HtmlAgilityPack.HtmlWeb();

            string result = "HAPxYahooFinance.Service1() result:\n";

            if (tickers.Count > 0)
            {
                foreach (string t in tickers)
                {
                    string url = urlBase + t;
                    var doc = web.Load(url);

                    Debug.WriteLine("debug: t = " + t);

                    try
                    {
                        HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectNodes(
                            "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[3]/div[1]/div/fin-streamer[1]").First();

                        result = result + t + ": " + node.Attributes["value"].Value + "\n";
                    }
                    catch (Exception ex)
                    {
                        result = result + "DocumentNode.SelectNodes failed for " + t + "\n";
                        Debug.WriteLine("DocumentNode.SelectNodes failed for " + t);
                    }

                    Debug.WriteLine("debug: result = " + result);

                }
            }

            return result;
        }

        public static string Service()
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            List<string> tickers = GetYahooTickers();
            var web = new HtmlAgilityPack.HtmlWeb();
            string result = "HAPxYahooFinance.Service() result:\n";

            if (tickers.Count > 0)
            {
                foreach(string t in tickers)
                {
                    string searchElementPriceBeg1 = "data-symbol=\"" + t + "\"";
                    string searchElementPriceBeg2 = ">";
                    string searchElementPriceEnd = "<";

                    string url = urlBase + t;
                    var doc = web.Load(url);
                    string docText = doc.Text;
                    result = result + "url: " + url + "\ndocText.Length: " + docText.Length.ToString()
                        + ", docText[..100]:\n" + docText.Substring(0, 100) + "\n";

                    int idxBeg = docText.IndexOf(searchElementPriceBeg1);
                    idxBeg = docText.IndexOf(searchElementPriceBeg2, idxBeg);
                    int idxEnd = docText.IndexOf(searchElementPriceEnd, idxBeg);

                    result = result + "idxBeg, idxEnd:\n" + idxBeg.ToString() + ", " + idxEnd.ToString() + "\n";

                    string data = docText.Substring(idxBeg + 1, idxEnd - idxBeg - 1);

                    result = result + "data: " + data + "\n";
                }
            }

            return result;
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
