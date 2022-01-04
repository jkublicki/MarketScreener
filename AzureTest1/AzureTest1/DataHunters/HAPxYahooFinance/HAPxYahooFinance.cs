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

        private static void ServiceDeadUrl(string ticker)
        {
            if (QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, String.Concat(
                "UPDATE ENU_TICKER SET UpdateDate = GETUTCDATE() WHERE TickerYahoo = '", ticker, "'"), false, out bool _) == -1)
                Log.Entry(String.Concat("HAPxYahooFinance.Service() is stuck! Can't service and can't update ", ticker));
        }

        public static void Service(List<string> tickers, int tickerSleepMs)
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            const bool skipDataExtraction = true;
            WebsiteNodes.WebsiteNodeSet yahooEquityNodeSet = HAPxYFSettings.YahooEquityNodeSet();

            int failCount = 0;

            HtmlAgilityPack.HtmlWeb web = new();
            web.PreRequest = OnPreRequest;

            if (Log.Enabled) Log.Entry("HAPxYahooFinance service result:");

            if (web == null)
            {
                if (Log.Enabled) Log.Entry("   HtmlAgilityPack.HtmlWeb() failed");
                failCount++;
                return;
            }

            if (tickers.Count > 0)
            {
                foreach (string t in tickers)
                {
                    string url = urlBase + t;

                    HtmlAgilityPack.HtmlDocument? doc;

                    try
                    {                        
                        doc = web.Load(url);                        
                    }
                    catch (Exception e)
                    {
                        if (Log.Enabled) Log.Entry(String.Concat("   Skipping ticker ", t, ", HtmlAgilityPack.HtmlWeb.Load() failed for url ", url,
                            ". Full exception: ", e.ToString(), ""));
                        failCount++;
                        ServiceDeadUrl(t);
                        continue;
                    }
                     
                    if (doc == null || doc.Text == null || doc.Text.Length < 100)
                    {   
                        if (Log.Enabled) Log.Entry(String.Concat("   Skipping ticker ", t, ", HtmlAgilityPack.HtmlWeb.Load() returned empty document from url ", url, ""));
                        failCount++;
                        ServiceDeadUrl(t);
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
                                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectSingleNode(n.FullXPATH);

                                string? dataPoint = null; 

                                if (node != null && n.DataLocation == WebsiteNodes.DataLocations.AttributeValue)
                                {
                                    try
                                    {
                                        if (node.HasAttributes && node.Attributes["value"] != null) 
                                        {
                                            dataPoint = node.Attributes["value"].Value; //tu potrafi polecieć exception pomimo try https://stackoverflow.com/questions/30498612/try-catch-doesnt-catch-exception
                                            n.Value = dataPoint;
                                        }
                                        else
                                        {
                                            if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (AttributeValue) failed - the node doesn't have a such attribute - for ", t, ", ", n.Name, ""));
                                            failCount++;
                                        }
                                    }
                                    catch (Exception e)
                                    {   
                                        if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (AttributeValue) failed - exception - for ", t, ", ", n.Name, ""));
                                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                        failCount++;
                                    }
                                }
                                else if (node != null && n.DataLocation == WebsiteNodes.DataLocations.InnerText)
                                {
                                    try
                                    {
                                        if (node.InnerText != null)
                                        {
                                            dataPoint = node.InnerText;
                                            n.Value = dataPoint;

                                        }
                                        else
                                        {
                                            if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerText) failed - InnerText is null - for ", t, ", ", n.Name, ""));
                                            failCount++;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerText) failed - exception - for ", t, ", ", n.Name, ""));
                                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                        failCount++;
                                    }
                                }
                                else if (node != null && n.DataLocation == WebsiteNodes.DataLocations.InnerHtml)
                                {
                                    try
                                    {
                                        if (node.InnerHtml != null)
                                        {
                                            dataPoint = node.InnerHtml;
                                            n.Value = dataPoint;
                                        }
                                        else
                                        {
                                            if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerHtml) failed - InnerHtml is null - for ", t, ", ", n.Name, ""));
                                            failCount++;
                                        }   
                                    }
                                    catch (Exception e)
                                    {
                                        if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerHtml) failed - exception - for ", t, ", ", n.Name, ""));
                                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                        failCount++;
                                    }
                                }

                                //debug
                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(String.Concat("  ", t, ", ", n.Name, ": ", dataPoint, ""));

                            }
                            catch (Exception e)
                            {
                                if (Log.Enabled) Log.Entry(String.Concat("  XPATH select failed for ", t, ", node ", n.Name, ""));
                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                failCount++;
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

                                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(String.Concat("  ", t, ", ", n.Name, ": ", dataPoint, ""));
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
                                if (Log.Enabled) Log.Entry(String.Concat("   Text search failed for ", t, ", node ", n.Name, ""));
                                failCount++;
                            }                            
                        }

                        
                        string s = NodeConverters.ConvertValue(n.Value, n.ConverterFunction, out bool su);
                        if (su)
                        {
                            n.Value = s;
                            if (Log.DebugEnabled && Log.Enabled) Log.Entry(String.Concat("  ", t, ", ", n.Name, ", converted value: ", n.Value, ""));
                        }
                        else
                        {
                            if (Log.Enabled) Log.Entry(String.Concat("  Conversion failure for value ", n.Value, ", converter ", n.ConverterFunction, ", node ", n.Name, ", ticker ", n.Ticker, ""));
                            failCount++;
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
                        {
                            if (Log.Enabled) Log.Entry(String.Concat("   QueryDatabase.ExecuteSQLStatement() failed for query: ", updateSQL, ""));
                            failCount++;
                        }   

                        int insertedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, insertSQL, false, out bool _);
                        if (insertedRows == -1)
                        {
                            if (Log.Enabled) Log.Entry(String.Concat("   QueryDatabase.ExecuteSQLStatement() failed for query: ", insertSQL, "")); //przydałoby się out message, a nie ten bool, ale co z przypadkiem data table? do wymyślenia ładniejsze rozwiązanie                                                                                                                            
                            failCount++;
                        }
                    }

                    decimal rand = (decimal)(new Random().NextDouble() * 0.2 + 1); //zgodnie z praktykami (nie za szybko, losowo) https://www.scrapehero.com/how-to-prevent-getting-blacklisted-while-scraping/
                    int sleep = (int)Math.Floor(tickerSleepMs * rand);                    
                    Thread.Sleep(sleep);
                }
            }

            if (failCount == 0 && tickers.Count > 0 && Log.Enabled) 
                Log.Entry("Complete without issues");
            else if (failCount == 0 && tickers.Count == 0 && Log.Enabled) 
                Log.Entry("Nothing to service");
            else if (Log.Enabled) Log.Entry(String.Concat("Complete with issues listed above"));
        }


    }
}
