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
 * //błąd: przypadek ATD.WA - nie ma recommendation rating, znajduje BeforeLeft i za nim Left, ale to nie ten co trzeba
 * //dorobić przeszukiwanie a la pierwszy Service() - przeszukanie html-a
 * //1. dorobić strukturę bazy 
 * //...i zapis do bazy
 * //- co z konwersją, np. 52w range?
 * //- potem uporządkować klasy node i nodeSet
 * 2. dorobić timer i powolne, odpowiedzialne działanie
 * 3. dorabiać kolejne node-y dla strony yahoo finance i rozwiązywać na bieżąco problemy z uzyskaniem node-a wg. XPATH i wartości z node-a
 * 4. dorobić log
 * 
 * 
 * teraz: https://html-agility-pack.net/select-nodes
 * - bo se nie mają przyszłości i zaczęły powodować problemy
 * 
 * concaty
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

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal static class HAPxYahooFinance
    {
     

        public static string Service(List<string> tickers)
        {
            const string urlBase = "https://finance.yahoo.com/quote/";
            WebsiteNodes.WebsiteNodeSet yahooEquityNodeSet = HAPxYFSettings.YahooEquityNodeSet();            

            var web = new HtmlAgilityPack.HtmlWeb();
            string result = "HAPxYahooFinance.Service() result:\n";

            if (tickers.Count > 0)
            {
                foreach (string t in tickers)
                {
                    string url = urlBase + t;
                    var doc = web.Load(url);
                    doc.Save("doc_" + t + ".txt");
                    //Debug.WriteLine("debug: t = " + t);
                    yahooEquityNodeSet.Ticker = t;

                    string updateSQL = "";
                    string insertSQLColumns = "";
                    string insertSQLValues = "";

                    foreach (WebsiteNodes.WebsiteNode n in yahooEquityNodeSet.Nodes)
                    {
                        n.Value = null; //bez tego przeniosłoby wartość z poprzedniego tickera!

                        n.Ticker = t;

                        //Debug.WriteLine("debug: node = " + n.Name + "\n");
                        if (n.ServiceMode == WebsiteNodes.ServiceModes.XPATH)
                        {                            
                            try
                            {
                                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectNodes(n.FullXPATH).First();
                                string? dataPoint = null; 

                                if (n.DataLocation == WebsiteNodes.DataLocations.AttributeValue)
                                {
                                    try
                                    {
                                        dataPoint = node.Attributes["value"].Value;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (AttributeValue) failed for " + t + ", " + n.Name + "\n";
                                        //Debug.WriteLine("Data point value acquisition (AttributeValue) failed for " + t + ", " + n.Name + "\n");
                                    }
                                }
                                else if (n.DataLocation == WebsiteNodes.DataLocations.InnerText)
                                {
                                    try
                                    {
                                        dataPoint = node.InnerText;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (InnerText) failed for " + t + ", " + n.Name + "\n";
                                        //Debug.WriteLine("Data point value acquisition (InnerText) failed for " + t + ", " + n.Name + "\n");
                                    }
                                }
                                else if (n.DataLocation == WebsiteNodes.DataLocations.InnerHtml)
                                {
                                    try
                                    {
                                        dataPoint = node.InnerHtml;
                                        n.Value = dataPoint;
                                    }
                                    catch (Exception)
                                    {
                                        result = result + "Data point value acquisition (InnerHtml) failed for " + t + ", " + n.Name + "\n";
                                        //Debug.WriteLine("Data point value acquisition (InnerHtml) failed for " + t + ", " + n.Name + "\n");
                                    }
                                }

                                //PRZYDATNE!!
                                //odkomentować do badania szczegółów - 1 z 2
                                result = result + t + ", " + n.Name + ": " + dataPoint + "\n";

                            }
                            catch (Exception)
                            {
                                result = result + "DocumentNode.SelectNodes failed for " + t + ", node " + n.Name + "\n";
                                //Debug.WriteLine("DocumentNode.SelectNodes failed for " + t + ", node " + n.Name + "\n");
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

                                if (idxBeg != -1)
                                {
                                    int idxEnd = docText.IndexOf(n.SearchElementRight, idxBeg + n.SearchElementLeft.Length);

                                    if (idxEnd != -1 && idxEnd - idxBeg < n.LeftSEMaxDistance)
                                    {
                                        string dataPoint = docText.Substring(idxBeg + n.SearchElementLeft.Length, idxEnd - (idxBeg + n.SearchElementLeft.Length));

                                        Console.WriteLine("... " + dataPoint);
                                        n.Value = dataPoint;
                                        Console.WriteLine("... " + n.Value);

                                        //PRZYDATNE!!
                                        //odkomentować do badania szczegółów - 2 z 2
                                        //result = result + t + ", " + n.Name + ": " + dataPoint + "\n";

                                        Console.WriteLine(t + ", " + n.Name + ": " + dataPoint + "\n");

                                        //Debug.WriteLine(t + " search indexes: " + idxBeg.ToString() + ", " + idxEnd.ToString());
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
                                result = result + "Text search failed for " + t + ", node " + n.Name + "\n";
                                //Debug.WriteLine("Text search failed for " + t + ", node " + n.Name + "\n");
                            }                            
                        }

                        
                        //Debug.WriteLine("debug: result = " + result);



                        //to nadal są flaczki pętli po node-ach

                        string s = NodeConverters.ConvertValue(n.Value, n.ConverterFunction, out bool su);
                        if (su)
                            n.Value = s;
                        else
                        {                            
                            result += String.Concat("Conversion failure for value ", n.Value, ", converter ", n.ConverterFunction, ", node ", n.Name, ", ticker ", n.Ticker, "\n");
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

                    updateSQL = String.Concat("UPDATE ENU_TICKER SET ", updateSQL, "UpdateDate = '", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        "' WHERE TickerYahoo = '", t, "'");
                    string insertSQL = String.Concat("INSERT INTO TICKER_HISTORY (", insertSQLColumns, " UpdateDate, SnapshotDate, TickerGoogleFinance) VALUES (",
                        insertSQLValues, "'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "', '", DateTime.Today.ToString("yyyy-MM-dd"), 
                        "', (SELECT TOP 1 TickerGoogleFinance FROM ENU_TICKER WHERE TickerYahoo = '", t, "'))");


                    int updatedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, updateSQL, false, out bool _);
                    if (updatedRows == -1)
                        result += String.Concat("QueryDatabase.ExecuteSQLStatement() failed for query: ", updateSQL, "\n");
                    //else
                        //result += String.Concat("QueryDatabase.ExecuteSQLStatement() worked for query: ", updateSQL, "\n");


                    int insertedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, insertSQL, false, out bool _);
                    if (insertedRows == -1)
                        result += String.Concat("QueryDatabase.ExecuteSQLStatement() failed for query: ", insertSQL, "\n"); //przydałoby się out message, a nie ten bool, ale co z przypadkiem data table? do wymyślenia ładniejsze rozwiązanie
                    //else
                        //result += String.Concat("QueryDatabase.ExecuteSQLStatement() worked for query: ", insertSQL, "\n");


                }
            }

            return result;
        }


    }
}
