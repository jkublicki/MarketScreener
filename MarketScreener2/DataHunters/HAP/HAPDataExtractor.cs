using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

//wygląda na to, że jest problem z dynamicznym contentem
//https://html-agility-pack.net/knowledge-base/10169484/htmlagilitypack-and-dynamic-content-issue
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.webbrowser.readystate?view=windowsdesktop-6.0
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.webbrowserreadystate?view=windowsdesktop-6.0
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.webbrowser?view=windowsdesktop-6.0
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.webbrowser.isbusy?view=windowsdesktop-6.0#system-windows-forms-webbrowser-isbusy
//może kilka razy load i porównywać?
//może zapisywać strony w momencie pobrania
////dodać zakaż używania cache
//generalnie 13.01 o 10:37 PL YF wyświetla dane dla TROW, ZNGA za 10.01 !!!! - da się to rozpoznać po "At close: January 10 04:00PM EST" pod ceną

//usunąć z website element columnname i tablename, nie są używane

namespace MarketScreener.DataHunters.HAP
{
    internal class HAPDataExtractor
    {
        private static bool OnPreRequest(System.Net.HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;
            return true;
        }

        public void Extract(string urlKey, string url, WebsiteStructure websiteStructure, WebsiteDownloader downloader, ref HAPDiagnostics diagnostics)
        {
            /*
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb(); //web nie potrzebuje dispose, zostawić to GC: https://github.com/zzzprojects/html-agility-pack/issues/370
            web.PreRequest = OnPreRequest;
            web.UsingCache = false;
            web.UseCookies = false;

            if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("HAPDataExtractor.Extract() result for ", urlKey, ":"));

            if (web == null)
            {
                if (HAPSettings.LogEnabled) Log.Entry("   HtmlAgilityPack.HtmlWeb() failed");
                diagnostics.TechnicalFailsCount++;

                return;
            } 
            */

            DataStructure dataStructure = new DataStructure();
            dataStructure.UrlKey = urlKey;

            HtmlAgilityPack.HtmlDocument doc;

            /*
            try
            {
                var httpClient = new HttpClient();


                var response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode(); // Ensure success status code

                var html = response.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(html);



                //ex
                //doc = web.Load(url);
            }
            catch (Exception e)
            {
                if (HAPSettings.LogEnabled) Log.Entry(String.Concat("Skipping ticker ", urlKey, ", HtmlAgilityPack.HtmlWeb.Load() failed for url ", url,
                    ". Full exception: ", e.ToString(), ""));
                diagnostics.FailedUrlLoadsCount++;
                diagnostics.FailedUrlLoads.Add((urlKey, url));
                //nie ma servie dead url, nadrzędna klasa odpowiada za to, żeby wywołać extract z danym url tylko raz
                //wpis do logu o zakończeniu Extract()?
                return;
            }
            */

            string html = downloader.DownloadWebsite(url, out bool isOk);
            if (!isOk)
            {
                diagnostics.FailedUrlLoadsCount++;
                diagnostics.FailedUrlLoads.Add((urlKey, url));
                return;
            }

            if (HAPSettings.DebugEnabled)
            {
                string src = html.Substring(0, Math.Min(html.Length, 800));
                src = src.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
                Log.Entry($"HAPDataExtractor - html preview: " + src + "\n");
            }

            doc = new HtmlDocument();
            doc.LoadHtml(html);


            if (doc == null || doc.Text == null || doc.Text.Length < 100)
            {
                if (HAPSettings.LogEnabled) Log.Entry(String.Concat("Skipping ticker ", urlKey, ", HtmlAgilityPack.HtmlWeb.Load() returned empty document from url ", url, "\n"));
                diagnostics.DeadUrlCount++;
                diagnostics.DeadUrls.Add((urlKey, url));    
                //nie ma servie dead url, nadrzędna klasa odpowiada za to, żeby wywołać extract z danym url tylko raz
                //wpis do logu o zakończeniu Extract()?
                return;
            }

            diagnostics.LoadedUrlsCount++;

            //debug
            //tymczasowo włączone zapisywanie do czasu wyjaśnienia problemów z błędnymi danymi
            //if (HAPSettings.DebugEnabled)
            //    doc.Save("doc_" + urlKey + ".txt");


            foreach (WebsiteElement n in websiteStructure.WebsiteElements)
            {
                DataField dataField = new DataField();
                dataField.Name = n.Name;    

                if (n.ServiceMode == WebsiteElement.ServiceModes.XPATH)
                {
                    HtmlAgilityPack.HtmlNode node = null;

                    try
                    {
                        node = doc.DocumentNode.SelectSingleNode(n.XPATH);

                        string dataPoint = null;

                        if (node == null)
                        {
                            if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("XPATH select failed - node not found - for ", urlKey, ", node ", n.Name, "\n"));
                            diagnostics.FindElementFailsCount++;
                        }
                        else
                        {
                            dataField.WebsiteElementFound = true;
                            diagnostics.ElementsFoundCount++;
                        }

                        if (node != null && n.DataLocation == WebsiteElement.DataLocations.AttributeValue)
                        {
                            try
                            {
                                if (node.HasAttributes && node.Attributes["value"] != null)
                                {
                                    dataPoint = node.Attributes["value"].Value; //tu potrafi polecieć exception pomimo try https://stackoverflow.com/questions/30498612/try-catch-doesnt-catch-exception
                                    dataField.Value = dataPoint;
                                    diagnostics.ExtractedDataCount++;
                                }
                                else
                                {
                                    if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (AttributeValue) failed - the node doesn't have a such attribute - for ", urlKey, ", ", n.Name, "\n"));
                                    diagnostics.ExtractDataFailsCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (AttributeValue) failed - exception - for ", urlKey, ", ", n.Name, "\n"));
                                if (HAPSettings.DebugEnabled) Log.Entry(e.ToString() + "\n");
                                diagnostics.ExtractDataFailsCount++;
                            }
                        }
                        else if (node != null && n.DataLocation == WebsiteElement.DataLocations.InnerText)
                        {
                            try
                            {
                                if (node.InnerText != null)
                                {
                                    dataPoint = node.InnerText;
                                    dataField.Value = dataPoint;
                                    diagnostics.ExtractedDataCount++;
                                }
                                else
                                {
                                    if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (InnerText) failed - InnerText is null - for ", urlKey, ", ", n.Name, "\n"));
                                    diagnostics.ExtractDataFailsCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (InnerText) failed - exception - for ", urlKey, ", ", n.Name, "\n"));
                                if (HAPSettings.DebugEnabled) Log.Entry(e.ToString() + "\n");
                                diagnostics.ExtractDataFailsCount++;
                            }
                        }
                        else if (node != null && n.DataLocation == WebsiteElement.DataLocations.InnerHtml)
                        {
                            try
                            {
                                if (node.InnerHtml != null)
                                {
                                    dataPoint = node.InnerHtml;
                                    dataField.Value = dataPoint;
                                    diagnostics.ExtractedDataCount++;
                                }
                                else
                                {
                                    if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (InnerHtml) failed - InnerHtml is null - for ", urlKey, ", ", n.Name, "\n"));
                                    diagnostics.ExtractDataFailsCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Value acquisition (InnerHtml) failed - exception - for ", urlKey, ", ", n.Name, "\n"));
                                if (HAPSettings.DebugEnabled) Log.Entry(e.ToString() + "\n");
                                diagnostics.ExtractDataFailsCount++;
                            }
                        }

                        //debug
                        if (HAPSettings.DebugEnabled && HAPSettings.DebugEnabled) Log.Entry(String.Concat("", urlKey, ", ", n.Name, ": ", dataPoint, "\n"));

                    }
                    catch (Exception e)
                    {
                        if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("XPATH select failed - exception - for ", urlKey, ", node ", n.Name, "\n"));
                        if (HAPSettings.DebugEnabled) Log.Entry(e.ToString() + "\n");
                        diagnostics.FindElementFailsCount++;
                    }
                }
                else if (n.ServiceMode == WebsiteElement.ServiceModes.DOCTEXT)
                {
                    string docText = doc.Text;
                    int idxBeg0 = docText.IndexOf(n.SearchElementBeforeLeft);
                    bool success = true;

                    if (idxBeg0 != -1)
                    {
                        int idxBeg = docText.IndexOf(n.SearchElementLeft, idxBeg0);

                        if (idxBeg != -1 && idxBeg - idxBeg0 < n.LeftSEMaxDistance) 
                        {
                            int idxEnd = docText.IndexOf(n.SearchElementRight, idxBeg + n.SearchElementLeft.Length);

                            if (idxEnd != -1 && idxEnd - idxBeg < n.RightSEMaxDistance)
                            {
                                string dataPoint = docText.Substring(idxBeg + n.SearchElementLeft.Length, idxEnd - (idxBeg + n.SearchElementLeft.Length));
                                dataField.Value = dataPoint;
                                diagnostics.ElementsFoundCount++;
                                diagnostics.ExtractedDataCount++;

                                if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("  ", urlKey, ", ", n.Name, ": ", dataPoint, ""));
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
                        if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Text search failed for ", urlKey, ", node ", n.Name, "\n"));
                        diagnostics.FindElementFailsCount++;
                        //diagnostics.ExtractDataFailsCount++;
                    }
                    else
                        dataField.WebsiteElementFound = true;
                }

                

                //-- CONVERSION
                if (dataField.WebsiteElementFound)
                {
                    
                    string s = StringConverters.ConvertValue(dataField.Value, n.ConvertingFunction, n.ExtraParam, out bool su);
                    if (su)
                    {
                        dataField.Value = s;
                        diagnostics.ExecutedConversionsCount++;
                        
                        if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("", urlKey, ", ", n.Name, ", converted value: ", dataField.Value, "\n"));
                    }
                    else
                    {
                        if (HAPSettings.DebugEnabled) Log.Entry(String.Concat("Conversion failure for value ", dataField.Value, ", converter ", n.ConvertingFunction, ", node ", n.Name, ", ticker ", urlKey, "\n"));
                        diagnostics.FailedConversionsCount++;
                        diagnostics.FailedConversions.Add((dataField.Value, n.ConvertingFunction));

                        dataField.Value = "NULL";
                    }
                }
                else
                {
                    dataField.Value = "NULL";
                }

                dataStructure.DataFields.Add(dataField);

                
            }

            if (dataStructure.DataFields.Where(f => f.Value != "NULL").Count() < dataStructure.DataFields.Where(f => f.Value == "NULL").Count())
            {
                diagnostics.BrokenWebsitesCount++;
                diagnostics.BrokenWebsites.Add((urlKey, url));
                //if (HAPSettings.SaveBrokenWebsites)
                //    doc.Save("broken_doc_" + urlKey + ".txt");
                if (HAPSettings.LogEnabled) Log.Entry(String.Concat("", urlKey, ", numerous failures in finding website elements.\n"));
            }

            //założenia:
            //SQL-e są zdefiniowane w WebsiteStructure, są częścią planu czytania podstron o jednakowej strukturze
            //SQL-e zawierają nazwy WebsiteElement-ów/DataField-ów (to ta sama wartość) obramowane nawiasami klamrowymi
            //SQL-e zawierają fragment {UrlKey}
            //fragmenty w klamrach zostaną podmienione
            //np. UPDATE ENU_TICKER SET Volume = {Volume} WHERE TickerYF = {UrlKey} -> UPDATE ENU_TICKER SET Volume = 1000 WHERE TickerYF = 'ATD.WA'
            //autor planu jest odpowiedzialny za rozwiązywanie problemów z SQL, np. przez stosowanie IF (...) UPDATE ... ELSE INSERT ...
            //autor planu lub operator bazy jest odpowiedzialny za konwersję danych z odczytanych na użyteczne, konwersja tutaj ma tylko na celu umożliwienie zapisu

            List<Tuple<string, string>> statements = new List<Tuple<string, string>>();

            foreach (Tuple<string, string> statement in websiteStructure.SQLStatements)
            {
                string newStatement = statement.Item1;

                foreach (DataField field in dataStructure.DataFields)
                {                    
                    string s = String.Concat(@"{", field.Name, @"}");

                    if (newStatement.Contains(s))
                        newStatement = newStatement.Replace(s, field.Value);                    
                }

                if (newStatement.Contains(@"{UrlKey}"))
                    newStatement = newStatement.Replace(@"{UrlKey}", urlKey);

                statements.Add(Tuple.Create(newStatement, statement.Item2));
            }

            //todo: dodać wykonanie statements albo przekazanie wyżej do puli do wykonania


            //todo: dodać rozpoznawanie statements związanych ze zdarzeniami innymi niż zakończenie i rozpoznawanie zdarzeń, np. url in dead/broken

            if (!HAPSettings.SkipDataExtraction)
            {
                List<string> statementsToRun = new List<string>();
                statementsToRun.AddRange(statements.Where(p => p.Item2 == "OnExtractFinished").Select(p => p.Item1).ToList());
                if (diagnostics.DeadUrls.Any(p => p.Item1 == urlKey))
                {
                    statementsToRun.AddRange(statements.Where(p => p.Item2 == "OnDeadUrl").Select(p => p.Item1).ToList());
                }
                else if (diagnostics.BrokenWebsites.Any(p => p.Item1 == urlKey))
                {
                    statementsToRun.AddRange(statements.Where(p => p.Item2 == "OnBrokenUrl").Select(p => p.Item1).ToList());
                }

                List<int> insertedRowsList = QueryDatabase.ExecuteSQLStatementNQ(Secrets.ConnectionString, 
                    statementsToRun, false);

                for (int i = 0; i < insertedRowsList.Count; i++)
                {
                    if (insertedRowsList[i] == -1)
                    {
                        diagnostics.FailedSQLStatementsCount++;
                        diagnostics.FailedSQLStatements.Add((statementsToRun[i], DateTime.UtcNow));
                    }
                    else
                        diagnostics.ExecutedSQLStatementsCount++;
                }
            }

            

            //zalogowanie zakończenia
            if (HAPSettings.LogEnabled)
                Log.Entry(String.Concat("Extraction complete (", urlKey, ")\n"));
           
        }
    }
}
