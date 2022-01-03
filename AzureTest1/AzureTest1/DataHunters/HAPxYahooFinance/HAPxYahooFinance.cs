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
 * BUGI
 * - Po dodaniu w settings node-ów Currency i CompanyName leci paskudny null, nie mogę znaleźć przyczyny
 * -- Okazało się, że próba pobrania atrybutu value z node-a, któy go nie ma, skutkuje wyjątkiem, którego try catch nie łapie
 * -- Dodałem obsługę wyjątków, teraz jest nowy błąd (może niezwiązany ze zmianami) - program chyba się gdzieś blokuje permanentnie w dziwnym stanie
 * --- Widać w konsoli debug gdzie
 * --- Ze względu na inny błąd (błąd linq) musiałem wyłaczyć przycinanie w konwerterze Varchar50, co potem spowoduje problem z zapisem do bazy
 * 
 * 
 * - Drobny: po północy loguje nadal do pliku z poprzedniego dnia
 * 
 * KOD
 * - Chyba drastycznie nadużyłem static, kod nie nadaje się do pokazania nikomu
 * 
 * TODO
 *  
 * - Brakujące elementy strony: kraj, nazwa firmy, waluta, czy pobrano dane po czy przed zamknięciem, ilość analiz
 * -- Po zakończeniu przywrócić normalne działanie z obecnych ustawień debugowych:
 * --- usunąć nadpisanie tickers w managerze, wyłączyć skip data extraction w service, wyłączyć debug enabled w log

 * - Sprawdzenie poprawności danych zapisanych do bazy, wyszukiwanie przekłamań
 * -- skrypt Przydasie
 * -- porównanie z API google finance (select z bazy wkleić do google spreadsheet-a z googlefinance())
 * 
 * 
 * 
 */

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal static class HAPxYahooFinance
    {

        private static bool OnPreRequest(System.Net.HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;
            return true;
        }

        private static void ServiceDeadUrl(string ticker, ref string result)
        {
            if (QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, String.Concat(
                        "UPDATE ENU_TICKER SET UpdateDate = GETUTCDATE() WHERE TickerYahoo = '", ticker, "'"), false, out bool _) == -1)
                result = String.Concat(result, "    HAPxYahooFinance.Service() is stuck! Can't service and can't update ", ticker);
        }

        public static string Service(List<string> tickers, int tickerSleepMs)
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            const bool skipDataExtraction = true;

            WebsiteNodes.WebsiteNodeSet yahooEquityNodeSet = HAPxYFSettings.YahooEquityNodeSet();

            string result = "";

            HtmlAgilityPack.HtmlWeb web = new();
            web.PreRequest = OnPreRequest;

            

            if (web == null)
            {
                result = String.Concat(result, "   HtmlAgilityPack.HtmlWeb() failed\n");
                return result;
            }

            if (tickers.Count > 0)
            {
                foreach (string t in tickers)
                {
                    string url = urlBase + t;

                    HtmlAgilityPack.HtmlDocument? doc;

                    bool loadException = false;

                    try
                    {                        
                        doc = web.Load(url);                        
                    }
                    catch (Exception ex)
                    {
                        result = String.Concat(result, "   Skipping ticker ", t, ", HtmlAgilityPack.HtmlWeb.Load() failed for url ", url, 
                            ". Full exception: ", ex, "\n");
                        loadException = true;
                        ServiceDeadUrl(t, ref result);
                        continue;
                    }
                     
                    if (doc == null || doc.Text == null || doc.Text.Length < 100)
                    {
                        result = String.Concat(result, "   Skipping ticker ", t, ", HtmlAgilityPack.HtmlWeb.Load() returned empty document from url ", url, "\n");
                        ServiceDeadUrl(t, ref result);
                        continue;
                    }

                    //debug
                    if (Log.DebugEnabled)
                        doc.Save("doc_" + t + ".txt");

                    yahooEquityNodeSet.Ticker = t;

                    string updateSQL = "";
                    string insertSQLColumns = "";
                    string insertSQLValues = "";

                    foreach (WebsiteNodes.WebsiteNode n in yahooEquityNodeSet.Nodes)
                    {
                        n.Value = null; //bez tego przeniosłoby wartość z poprzedniego tickera!
                        n.Ticker = t;

                        if (n.ServiceMode == WebsiteNodes.ServiceModes.XPATH)
                        {                            
                            try
                            {
                                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectNodes(n.FullXPATH).First();
                                string? dataPoint = null; 

                                if (n.DataLocation == WebsiteNodes.DataLocations.AttributeValue)
                                {
                                    Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.AttributeValue 1");


                                    try
                                    {
                                        if (node.HasAttributes && node.Attributes["value"] != null)
                                        {
                                            dataPoint = node.Attributes["value"].Value; //tu potrafi polecieć exception pomimo try https://stackoverflow.com/questions/30498612/try-catch-doesnt-catch-exception
                                            n.Value = dataPoint;

                                            Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.AttributeValue 2");
                                        }
                                        else
                                            result = String.Concat(result, "   Value acquisition (AttributeValue) failed - the node doesn't have a such attribute - for ", t, ", ", n.Name, "\n");

                                        Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.AttributeValue 3");
                                    }
                                    catch (Exception)
                                    {
                                        result = String.Concat(result, "   Value acquisition (AttributeValue) failed - exception - for ", t, ", ", n.Name, "\n");
                                    }
                                }
                                else if (n.DataLocation == WebsiteNodes.DataLocations.InnerText)
                                {
                                    Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerText 1");

                                    try
                                    {
                                        if (node.InnerText != null)
                                        {
                                            dataPoint = node.InnerText;
                                            n.Value = dataPoint;

                                            Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerText 2");

                                        }
                                        else
                                            result = String.Concat(result, "   Value acquisition (InnerText) failed - InnerText is null - for ", t, ", ", n.Name, "\n");

                                        Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerText 3");
                                    }
                                    catch (Exception)
                                    {
                                        result = String.Concat(result, "   Value acquisition (InnerText) failed - exception - for ", t, ", ", n.Name, "\n");
                                    }
                                }
                                else if (n.DataLocation == WebsiteNodes.DataLocations.InnerHtml)
                                {
                                    Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerHtml 1");

                                    try
                                    {
                                        if (node.InnerHtml != null)
                                        {
                                            dataPoint = node.InnerHtml;
                                            n.Value = dataPoint;

                                            Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerHtml 2");

                                        }
                                        else
                                            result = String.Concat(result, "   Value acquisition (InnerHtml) failed - InnerHtml is null - for ", t, ", ", n.Name, "\n");

                                        Debug.WriteLine(n.Name + " wewnątrz n.DataLocation == WebsiteNodes.DataLocations.InnerHtml 3");

                                    }
                                    catch (Exception)
                                    {
                                        result = String.Concat(result, "   Value acquisition (InnerHtml) failed - exception - for ", t, ", ", n.Name, "\n");
                                    }
                                }

                                //debug
                                if (Log.DebugEnabled)
                                    result = String.Concat(result, "   ", t, ", ", n.Name, ": ", dataPoint, "\n");

                            }
                            catch (Exception)
                            {
                                result = String.Concat(result, "   XPATH select failed for ", t, ", node ", n.Name, "\n");
                            }
                        }
                        else if (n.ServiceMode == WebsiteNodes.ServiceModes.DOCTEXT)
                        {
                            string docText = doc.Text;
                            int idxBeg0 = docText.IndexOf(n.SearchElementBeforeLeft);
                            bool success = true;
                            
                            if (idxBeg0 != -1)
                            {
                                int idxBeg = docText.IndexOf(n.SearchElementLeft, idxBeg0);

                                if (idxBeg != -1 && idxBeg - idxBeg0 < n.LeftSEMaxDistance) //nowa zmiana..
                                {
                                    int idxEnd = docText.IndexOf(n.SearchElementRight, idxBeg + n.SearchElementLeft.Length);

                                    if (idxEnd != -1 && idxEnd - idxBeg < n.LeftSEMaxDistance) 
                                    {
                                        string dataPoint = docText.Substring(idxBeg + n.SearchElementLeft.Length, idxEnd - (idxBeg + n.SearchElementLeft.Length));
                                        n.Value = dataPoint;

                                        //debug                                        
                                        if (Log.DebugEnabled)
                                            result = String.Concat(result, "   ", t, ", ", n.Name, ": ", dataPoint, "\n");
                                    }
                                    else
                                        success = false;                
                                }
                                else
                                    success = false;
                            }
                            else
                                success = false;

                            if (!success)
                            {
                                result = String.Concat(result, "   Text search failed for ", t, ", node ", n.Name, "\n");
                            }                            
                        }

                        
                        string s = NodeConverters.ConvertValue(n.Value, n.ConverterFunction, out bool su);
                        if (su)
                        {
                            n.Value = s;
                            if (Log.DebugEnabled)
                                result = String.Concat(result, "    ", t, ", ", n.Name, ", converted value: ", n.Value, "\n");
                        }
                        else
                        {
                            result = String.Concat(result, "   Conversion failure for value ", n.Value, ", converter ", n.ConverterFunction, ", node ", n.Name, ", ticker ", n.Ticker, "\n");
                            n.Value = "NULL";
                        }

                        if (n.Tables.Contains("ENU_TICKER"))
                            updateSQL += String.Concat(n.ColumnName, " = ", n.Value, ", ");

                        if (n.Tables.Contains("TICKER_HISTORY"))
                        {
                            insertSQLColumns += String.Concat(n.ColumnName, ", ");
                            insertSQLValues += String.Concat(n.Value, ", ");
                        }
                    }

                    //tutaj jest komplet danych dla tickera, siedzi w node-ach w node-secie 

                    updateSQL = String.Concat("UPDATE ENU_TICKER SET ", updateSQL, "UpdateDate = '", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        "' WHERE TickerYahoo = '", t, "'");
                    string insertSQL = String.Concat("INSERT INTO TICKER_HISTORY (", insertSQLColumns, " UpdateDate, SnapshotDate, TickerGoogleFinance) VALUES (",
                        insertSQLValues, "'", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), "', '", DateTime.UtcNow.Date.ToString("yyyy-MM-dd"), 
                        "', (SELECT TOP 1 TickerGoogleFinance FROM ENU_TICKER WHERE TickerYahoo = '", t, "'))");

                    if (!skipDataExtraction)
                    {
                        int updatedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, updateSQL, false, out bool _);
                        if (updatedRows == -1)
                            result = String.Concat(result, "   QueryDatabase.ExecuteSQLStatement() failed for query: ", updateSQL, "\n");

                        int insertedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, insertSQL, false, out bool _);
                        if (insertedRows == -1)
                            result = String.Concat(result, "   QueryDatabase.ExecuteSQLStatement() failed for query: ", insertSQL, "\n"); //przydałoby się out message, a nie ten bool, ale co z przypadkiem data table? do wymyślenia ładniejsze rozwiązanie                                                                                                                            

                    }


                    decimal rand = (decimal)(new Random().NextDouble() * 0.2 + 1); //zgodnie z praktykami (nie za szybko, losowo) https://www.scrapehero.com/how-to-prevent-getting-blacklisted-while-scraping/
                    int sleep = (int)Math.Floor(tickerSleepMs * rand);                    
                    Thread.Sleep(sleep);
                }
            }

            if (result == "" && tickers.Count > 0)
                result = "HAPxYahooFinance.Service() result: OK\n";
            else if (result == "" && tickers.Count == 0)
                result = "HAPxYahooFinance.Service() result: nothing to service\n";
            else
                result = String.Concat("HAPxYahooFinance.Service() result:\n", result);

            return  result;
        }


    }
}
