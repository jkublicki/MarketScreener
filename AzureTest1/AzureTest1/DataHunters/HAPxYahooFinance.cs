using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;


/*
 * todo:
 * 
 * błąd: przypadek ATD.WA - nie ma recommendation rating, znajduje BeforeLeft i za nim Left, ale to nie ten co trzeba
 * dorobić przeszukiwanie a la pierwszy Service() - przeszukanie html-a
 * 1. dorobić strukturę bazy i zapis do bazy
 * - co z konwersją, np. 52w range?
 * - potem uporządkować klasy node i nodeSet
 * 2. dorobić timer i powolne, odpowiedzialne działanie
 * 3. dorabiać kolejne node-y dla strony yahoo finance i rozwiązywać na bieżąco problemy z uzyskaniem node-a wg. XPATH i wartości z node-a
 * 4. dorobić log
 * 
 * 
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
 * aby uniknąć bana od yahoo crawling powinien być podzielony na kawałki (mniejsze niż 1000)
 * timer https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-6.0
 * 
 * rozróżnić listę node-ów do zbadania i listę danych do insertowania
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

        private class WebsiteNode
        {
            public string Website;
            public string Ticker;
            public string Name;
            public string ServiceMode; //określa metodę szukania danych: XPATH, DOCTEXT
            public string FullXPATH;
            public string DataLocation; //określa położenie danych w node, tylko dla XPATH: AttributeValue, InnerText, InnerHtml            
            public string SearchElementBeforeLeft; //jn. poprzedzający SearchElementLeft
            public string SearchElementLeft; //charakterystyczny stały tekst poprzedzający dane, tylko dla DOCTEXT
            public int LeftSEMaxDistance;
            public string SearchElementRight; //jw. występujący po danych
            public string Value;
            public string ColumnName;
            public List<string> Tables; //zakładając, że update wielu tabel, ale tabele mają tak samo nazwane kolumny odpowiadające temu punktowi danych
            public string Type; //to jakiś bullshit, trzeba wymyślić jak określać konwersję
            public DateTime UpdateDate;
        }

        private class WebsiteNodeSet
        {
            public string Website;
            public string WebsiteType;
            public string Ticker;
            public DateTime UpdateDate;
            public List<WebsiteNode> Nodes;
        }

        private static WebsiteNodeSet yahooEquityNodeSet = new WebsiteNodeSet();

        private static void CreateNodeSets()
        {
            yahooEquityNodeSet = new()
            {
                Website = "finance.yahoo.com",
                WebsiteType = "Equity",
                UpdateDate = DateTime.Now,
                Nodes = new List<WebsiteNode>()
                {
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Price",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[3]/div[1]/div/fin-streamer[1]",
                        DataLocation = "AttributeValue",
                        ColumnName = "Price",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "decimal"
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "PreviousClose",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[1]/td[2]",
                        DataLocation = "InnerText",
                        ColumnName = "PreviousClose",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "decimal"
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "1yTargetEst",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[8]/td[2]",
                        DataLocation = "InnerText",
                        ColumnName = "1TargetEst",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "decimal"
                    }                    ,
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "52WeekRange",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                        DataLocation = "InnerText",
                        ColumnName = "52WeekRange",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "..."
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Volume",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[7]/td[2]/fin-streamer",
                        DataLocation = "InnerHtml",
                        ColumnName = "Volume",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "..."
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "AvgVolume",
                        ServiceMode = "XPATH",
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[8]/td[2]",
                        DataLocation = "InnerText",
                        ColumnName = "AvgVolume",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "decimal"
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Sector",
                        ServiceMode = "DOCTEXT",
                        SearchElementLeft = "sector\":\"",
                        SearchElementBeforeLeft = "summaryProfile",
                        LeftSEMaxDistance = 40,
                        SearchElementRight = "\",\"",
                        ColumnName = "Sector",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "string"
                    },
                    new WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "RecommendationRating",
                        ServiceMode = "DOCTEXT",
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "recommendationMean",
                        LeftSEMaxDistance = 10,
                        SearchElementRight = ",\"",
                        ColumnName = "Sector",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        Type = "string"
                    }



                }
            };
        }

        public static string Service1a()
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            List<string> tickers = GetYahooTickers();
            CreateNodeSets();
            

            var web = new HtmlAgilityPack.HtmlWeb();

            string result = "HAPxYahooFinance.Service1() result:\n";

            if (tickers.Count > 0)
            {
                foreach (string t in tickers)
                {
                    string url = urlBase + t;
                    var doc = web.Load(url);

                    doc.Save("doc_" + t + ".txt");

                    Debug.WriteLine("debug: t = " + t);

                    yahooEquityNodeSet.Ticker = t;
                    
                    

                    
                    foreach (WebsiteNode n in yahooEquityNodeSet.Nodes)
                    {
                        Debug.WriteLine("debug: node = " + n.Name + "\n");

                        if (n.ServiceMode == "XPATH")
                        {                            
                            try
                            {
                                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectNodes(
                                    n.FullXPATH
                                    ).First();

                                string dataPoint = "NULL";

                                if (n.DataLocation == "AttributeValue")
                                {
                                    try
                                    {
                                        dataPoint = node.Attributes["value"].Value;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (AttributeValue) failed for " + t + ", " + n.Name + "\n";
                                        Debug.WriteLine("Data point value acquisition (AttributeValue) failed for " + t + ", " + n.Name + "\n");
                                    }

                                }
                                else if (n.DataLocation == "InnerText")
                                {
                                    try
                                    {
                                        dataPoint = node.InnerText;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (InnerText) failed for " + t + ", " + n.Name + "\n";
                                        Debug.WriteLine("Data point value acquisition (InnerText) failed for " + t + ", " + n.Name + "\n");
                                    }
                                }
                                else if (n.DataLocation == "InnerHtml")
                                {
                                    try
                                    {
                                        dataPoint = node.InnerHtml;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (InnerHtml) failed for " + t + ", " + n.Name + "\n";
                                        Debug.WriteLine("Data point value acquisition (InnerHtml) failed for " + t + ", " + n.Name + "\n");
                                    }
                                }


                                result = result + t + ", " + n.Name + ": " + dataPoint + "\n";

                            }
                            catch (Exception)
                            {
                                result = result + "DocumentNode.SelectNodes failed for " + t + ", node " + n.Name + "\n";
                                Debug.WriteLine("DocumentNode.SelectNodes failed for " + t + ", node " + n.Name + "\n");
                            }
                        }
                        else if (n.ServiceMode == "DOCTEXT")
                        {
                            string docText = doc.Text;

                            int idxBeg0 = docText.IndexOf(n.SearchElementBeforeLeft);

                            bool success = true;
                            
                            if (idxBeg0 != -1)
                            {
                                int idxBeg = docText.IndexOf(n.SearchElementLeft, idxBeg0);

                                if (idxBeg != -1)
                                {
                                    int idxEnd = docText.IndexOf(n.SearchElementRight, idxBeg + n.SearchElementLeft.Length);

                                    if (idxEnd != -1 && idxEnd - idxBeg < n.LeftSEMaxDistance)
                                    {
                                        string dataPoint = docText.Substring(idxBeg + n.SearchElementLeft.Length, idxEnd - (idxBeg + n.SearchElementLeft.Length));

                                        n.Value = dataPoint;

                                        result = result + t + ", " + n.Name + ": " + dataPoint + "\n";

                                        Debug.WriteLine(t + " search indexes: " + idxBeg.ToString() + ", " + idxEnd.ToString());
                                    }
                                    else
                                    {
                                        success = false;
                                    }                                    
                                }
                                else
                                {
                                    success = false;
                                }
                            }
                            else
                            {
                               success = false;
                            }

                            if (!success)
                            {
                                result = result + "Text search failed for " + t + ", node " + n.Name + "\n";
                                Debug.WriteLine("Text search failed for " + t + ", node " + n.Name + "\n");
                            }
                            
                        }

                        
                    }

                    

                    Debug.WriteLine("debug: result = " + result);

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
