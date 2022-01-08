using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal class HAPDataExtractor
    {
        private static bool OnPreRequest(System.Net.HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;
            return true;
        }

        public void Extract(string urlKey, string url, WebsiteStructure websiteStructure)
        {
            int failCount = 0;
            int databaseFailCount = 0;

            HtmlAgilityPack.HtmlWeb web = new(); //web nie potrzebuje dispose, zostawić to GC: https://github.com/zzzprojects/html-agility-pack/issues/370
            web.PreRequest = OnPreRequest;

            if (Log.Enabled) Log.Entry("HAPDataExtractor.Extract() result:");

            if (web == null)
            {
                if (Log.Enabled) Log.Entry("   HtmlAgilityPack.HtmlWeb() failed");
                failCount++;
                return;
            }            

            DataStructure dataStructure = new();
            dataStructure.UrlKey = urlKey;

            HtmlAgilityPack.HtmlDocument? doc;

            try
            {
                doc = web.Load(url);
            }
            catch (Exception e)
            {
                if (Log.Enabled) Log.Entry(String.Concat("Skipping ticker ", urlKey, ", HtmlAgilityPack.HtmlWeb.Load() failed for url ", url,
                    ". Full exception: ", e.ToString(), ""));
                failCount++;
                //nie ma servie dead url, nadrzędna klasa odpowiada za to, żeby wywołać extract z danym url tylko raz
                //wpis do logu o zakończeniu Extract()?
                return;
            }

            if (doc == null || doc.Text == null || doc.Text.Length < 100)
            {
                if (Log.Enabled) Log.Entry(String.Concat("  Skipping ticker ", urlKey, ", HtmlAgilityPack.HtmlWeb.Load() returned empty document from url ", url, ""));
                failCount++;
                //nie ma servie dead url, nadrzędna klasa odpowiada za to, żeby wywołać extract z danym url tylko raz
                //wpis do logu o zakończeniu Extract()?
                return;
            }

            //debug
            if (Log.DebugEnabled)
                doc.Save("doc_" + urlKey + ".txt");


            foreach (WebsiteElement n in websiteStructure.WebsiteElements)
            {
                DataField dataField = new DataField();
                dataField.Name = n.Name;    

                if (n.ServiceMode == WebsiteElement.ServiceModes.XPATH)
                {
                    HtmlAgilityPack.HtmlNode? node = null;

                    try
                    {
                        node = doc.DocumentNode.SelectSingleNode(n.XPATH);

                        string? dataPoint = null;

                        if (node == null)
                        {
                            if (Log.Enabled) Log.Entry(String.Concat("  XPATH select failed - node not found - for ", urlKey, ", node ", n.Name, ""));
                            failCount++;
                            //dataField.Value = "NULL";
                            //dataField.WebsiteElementFound = false;
                        }
                        else
                            dataField.WebsiteElementFound = true;




                        if (node != null && n.DataLocation == WebsiteElement.DataLocations.AttributeValue)
                        {
                            try
                            {
                                if (node.HasAttributes && node.Attributes["value"] != null)
                                {
                                    dataPoint = node.Attributes["value"].Value; //tu potrafi polecieć exception pomimo try https://stackoverflow.com/questions/30498612/try-catch-doesnt-catch-exception
                                    dataField.Value = dataPoint;
                                }
                                else
                                {
                                    if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (AttributeValue) failed - the node doesn't have a such attribute - for ", urlKey, ", ", n.Name, ""));
                                    failCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (AttributeValue) failed - exception - for ", urlKey, ", ", n.Name, ""));
                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                failCount++;
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

                                }
                                else
                                {
                                    if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerText) failed - InnerText is null - for ", urlKey, ", ", n.Name, ""));
                                    failCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerText) failed - exception - for ", urlKey, ", ", n.Name, ""));
                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                failCount++;
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
                                }
                                else
                                {
                                    if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerHtml) failed - InnerHtml is null - for ", urlKey, ", ", n.Name, ""));
                                    failCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (Log.Enabled) Log.Entry(String.Concat("   Value acquisition (InnerHtml) failed - exception - for ", urlKey, ", ", n.Name, ""));
                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                                failCount++;
                            }
                        }

                        //debug
                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(String.Concat("  ", urlKey, ", ", n.Name, ": ", dataPoint, ""));

                    }
                    catch (Exception e)
                    {
                        if (Log.Enabled) Log.Entry(String.Concat("  XPATH select failed - exception - for ", urlKey, ", node ", n.Name, ""));
                        if (Log.Enabled && Log.DebugEnabled) Log.Entry(e.ToString());
                        failCount++;
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

                            if (idxEnd != -1 && idxEnd - idxBeg < n.LeftSEMaxDistance)
                            {
                                string dataPoint = docText.Substring(idxBeg + n.SearchElementLeft.Length, idxEnd - (idxBeg + n.SearchElementLeft.Length));
                                dataField.Value = dataPoint;

                                if (Log.Enabled && Log.DebugEnabled) Log.Entry(String.Concat("  ", urlKey, ", ", n.Name, ": ", dataPoint, ""));
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
                        if (Log.Enabled) Log.Entry(String.Concat("  Text search failed for ", urlKey, ", node ", n.Name, ""));
                        failCount++;
                    }
                    else
                        dataField.WebsiteElementFound = true;
                }

                //-- CONVERSION
                if (dataField.WebsiteElementFound)
                {
                    string s = StringConverters.ConvertValue(dataField.Value, n.ConverterFunction, n.ExtraParam, out bool su);
                    if (su)
                    {
                        dataField.Value = s;
                        if (Log.DebugEnabled && Log.Enabled) Log.Entry(String.Concat("  ", urlKey, ", ", n.Name, ", converted value: ", dataField.Value, ""));
                    }
                    else
                    {
                        if (Log.Enabled) Log.Entry(String.Concat("  Conversion failure for value ", dataField.Value, ", converter ", n.ConverterFunction, ", node ", n.Name, ", ticker ", urlKey, ""));
                        failCount++;
                        dataField.Value = "NULL";
                    }
                }
                else
                {
                    dataField.Value = "NULL";
                }

                dataStructure.DataFields.Add(dataField);

                
            }

            //założenia:
            //SQL-e są zdefiniowane w WebsiteStructure, są częścią planu czytania podstron o jednakowej strukturze
            //SQL-e zawierają nazwy WebsiteElement-ów/DataField-ów (to ta sama wartość) obramowane nawiasami klamrowymi
            //SQL-e zawierają fragment {UrlKey}
            //fragmenty w klamrach zostaną podmienione
            //np. UPDATE ENU_TICKER SET Volume = {Volume} WHERE TickerYF = {UrlKey} -> UPDATE ENU_TICKER SET Volume = 1000 WHERE TickerYF = 'ATD.WA'
            //autor planu jest odpowiedzialny za rozwiązywanie problemów z SQL, np. przez stosowanie IF (...) UPDATE ... ELSE INSERT ...
            //autor planu lub operator bazy jest odpowiedzialny za konwersję danych z odczytanych na użyteczne, konwersja tutaj ma tylko na celu umożliwienie zapisu

            List<string> statements = new();

            foreach (string statement in websiteStructure.SQLStatements)
            {
                string newStatement = statement;

                foreach (DataField field in dataStructure.DataFields)
                {                    
                    string s = String.Concat(@"{", field.Name, @"}");

                    if (newStatement.Contains(s))
                        newStatement = newStatement.Replace(s, field.Value);                    
                }

                if (newStatement.Contains(@"{UrlKey}"))
                    newStatement = newStatement.Replace(@"{UrlKey}", urlKey);

                statements.Add(newStatement);
            }

            //todo: dodać wykonanie statements albo przekazanie wyżej do puli do wykonania


            if (!HAPSettings.SkipDataExtraction)
            {
                foreach (string query in statements)
                {
                    int insertedRows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out bool _);
                    if (insertedRows == -1)
                    {
                        failCount++;
                        databaseFailCount++;
                    }
                }
            }



            //zalogowanie zakończenia
            if (failCount == 0 && Log.Enabled)
                Log.Entry("Complete without issues");
            else if (Log.Enabled) 
                Log.Entry(String.Concat("Complete with issues listed above"));

        }
    }
}
