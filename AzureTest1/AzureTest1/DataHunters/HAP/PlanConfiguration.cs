using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Oczekiwana struktura w bazie (schemat dbo):
 * MS_CFG_PLAN (PlanName string, RunDurationH int, UrlSetName string, IsActivePlan byte)
 * Jeden rekord MS_CFG_PLAN ma IsActivePlan = 1
 * Tablica lub widok o nazwie identycznej z MS_CFG_PLAN.UrlSetName (kolumny: UrlKey string, Url string)
 * ...
 */

namespace MarketScreener.DataHunters.HAP
{
    //pobiera z bazy konfigurację, waliduje i tworzy instancję website structure do użycia, przygotowuje SQL-a do url set-a
    internal class PlanConfiguration
    {
        //public static string? PlanName1 { get; set; }


        private string? planName = null;
        private int? runDurationH = null;
        private string? urlSetName = null;
        private WebsiteStructure? siteStructure = null;

        public WebsiteStructure SiteStructure
        {
            //TODO
            //konstruktor site structure to tempshit / ogólnie przemyśleć czy powinien być i coś sensownego zwracać, od pobrania struktury jest ta klasa
            get
            {
                if (siteStructure != null)
                {                    
                    return siteStructure;
                }
                    
                else
                {
                    ReadFromDatabase();
                    if (siteStructure != null)  
                        return siteStructure;
                    else
                        throw new Exception("RunConfiguration: siteStructure is null after ReadFromDatabase().");
                }
            }
        }

        public string PlanName
        {
            get
            {
                if (planName != null)
                    return planName;
                else
                {
                    ReadFromDatabase();
                    if (planName != null)
                        return planName;
                    else
                        throw new Exception("RunConfiguration: planName is null after ReadFromDatabase().");
                }
            }
            set
            {
                planName = value;
            }
        }
        public int RunDurationH 
        {
            get 
            {
                if (runDurationH != null)
                    return (int)runDurationH;
                else
                {
                    ReadFromDatabase();
                    if (runDurationH != null)
                        return (int)runDurationH;
                    else
                        throw new Exception("RunConfiguration: runDurationH is null after ReadFromDatabase().");
                }
            }
        }
        public string UrlSetName
        {
            get
            {
                if (urlSetName != null)
                    return urlSetName;
                else
                {
                    ReadFromDatabase();
                    if (runDurationH != null)
                        return urlSetName;
                    else
                        throw new Exception("RunConfiguration: runDurationH is null after ReadFromDatabase().");
                }
            }
        }

        private void ReadFromDatabase()
        {            
            if (planName != null)
            {
                string query1 = "SELECT TOP 1 PlanName FROM MS_CFG_PLAN WHERE IsActivePlan = 1";
                int rows1 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query1, false, out DataTable? dataTable1);
                if (rows1 == -1 || dataTable1 == null || dataTable1.Rows.Count == 0 || dataTable1.Rows[0].ItemArray.Length == 0 || dataTable1.Rows[0].IsNull(0))
                {
                    Log.Entry("Failed to find active plans in MS_CFG_PLAN. Run aborted.");
                    Environment.Exit(1);
                    return;
                }
                else
                    planName = dataTable1.Rows[0].ItemArray[0].ToString();
            }

            //poniżej wywalić wszystkie WHERE IsActivePlan, zastąpić nazwą planu


            string query2 = "SELECT TOP 1 PlanName, RunDurationH, UrlSetName FROM MS_CFG_PLAN WHERE PlanName = '" + planName + "'";
            int rows2 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query2, false, out DataTable? dataTable2);
            if (rows2 == -1 || dataTable2 == null)
            {
                Log.Entry("Failed to read plan from MS_CFG_PLAN. Run aborted.");
                Environment.Exit(1);
                return;
            }
            else
            {
                if (dataTable2.Rows.Count > 0 && dataTable2.Rows[0].ItemArray.Count() == 3)
                {
                    if (!dataTable2.Rows[0].IsNull(0))
                        planName = dataTable2.Rows[0].ItemArray[0].ToString();
                    else
                        throw new Exception("RunConfiguration: failed to read PlanName.");

                    if (!dataTable2.Rows[0].IsNull(1))
                    {
                        runDurationH = Convert.ToInt32(dataTable2.Rows[0].ItemArray[1]);
                        if (runDurationH <= 0)
                        {
                            Log.Entry("Plan configuration error: MS_CFG_PLAN.RunDurationH is null, 0, negative or theres some other issue with it. Run aborted.");
                            Environment.Exit(1);
                            return;
                        }
                    }                        
                    else
                        throw new Exception("RunConfiguration: failed to read RunDurationH.");

                    if (!dataTable2.Rows[0].IsNull(2))
                    {
                        urlSetName = dataTable2.Rows[0].ItemArray[2].ToString();

                        string query3 = String.Concat(@"SELECT IIF (EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'dbo' AND table_name = '", urlSetName, "'), 'Y', 'N')");

                        int rows3 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query3, false, out DataTable? dataTable3);
                        
                        if (rows3 == -1 || dataTable3 == null || dataTable3.Rows.Count == 0 || dataTable3.Rows[0].IsNull(0) || dataTable3.Rows[0].ItemArray[0].ToString() != "Y")
                        {
                            Log.Entry("Plan configuration error: can't to find url set (table or view) " + urlSetName + ", check i.a. user access. Run aborted.");
                            Environment.Exit(1);
                            return;
                        }

                        string query4 = String.Concat(@"select IIF (EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '", urlSetName, "' AND column_name = 'UrlKey' and ORDINAL_POSITION = 1), 'Y', 'N')");

                        int rows4 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query4, false, out DataTable? dataTable4);

                        if (rows4 == -1 || dataTable4 == null || dataTable4.Rows.Count == 0 || dataTable4.Rows[0].IsNull(0) || dataTable4.Rows[0].ItemArray[0].ToString() != "Y")
                        {
                            Log.Entry("Plan configuration error: first column of the url set " + urlSetName + " is not named UrlKey. Url set table should have two columns: UrlKey, Url. Run aborted.");
                            Environment.Exit(1);
                            return;
                        }

                        string query5 = String.Concat(@"select IIF (EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '", urlSetName, "' AND column_name = 'Url' and ORDINAL_POSITION = 2), 'Y', 'N')");

                        int rows5 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query5, false, out DataTable? dataTable5);

                        if (rows5 == -1 || dataTable5 == null || dataTable5.Rows.Count == 0 || dataTable5.Rows[0].IsNull(0) || dataTable5.Rows[0].ItemArray[0].ToString() != "Y")
                        {
                            Log.Entry("Plan configuration error: second column of the url set " + urlSetName + " is not named Url. Url set table should have two columns: UrlKey, Url. Run aborted.");
                            Environment.Exit(1);
                            return;
                        }
                    }
                    else
                        throw new Exception("RunConfiguration: failed to read UrlSetName.");
                }
                else
                    throw new Exception("RunConfiguration: no rows or wrong number of columns retrieved from MS_CFG_PLAN.");





            }

            //docelowo analogicznie jw. dla website structure i website elements
            //dodatkowo min i max time dla randomowego czekania
            string query6 = "SELECT Name, ServiceMode, XPATH, DataLocation, SearchElementBeforeLeft, SearchElementLeft, LeftSEMaxDistance, RightSEMaxDistance, SearchElementRight, ConvertingFunction, ExtraParam FROM MS_CFG_WEBSITE_ELEMENT WHERE PlanName = '" + planName + "'";
            int rows6 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query6, false, out DataTable? dataTable6);
            if (rows6 == -1 || dataTable6 == null)
            {
                Log.Entry("Failed to read from MS_CFG_WEBSITE_ELEMENT. Run aborted.");
                Environment.Exit(1);
                return;
            }
            else
            {
                if (dataTable6.Rows.Count > 0 && dataTable6.Rows[0].ItemArray.Count() == 11)
                {
                    WebsiteStructure structure = new();

                    foreach(DataRow row in dataTable6.Rows)
                    {
                        WebsiteElement element = new WebsiteElement();

                        if (row["Name"] != null && !row.IsNull("Name"))
                            element.Name = row["Name"].ToString();
                        if (row["ServiceMode"] != null && !row.IsNull("ServiceMode"))
                        {
                            if (WebsiteElement.StringServiceModes.ContainsKey(row["ServiceMode"].ToString()))
                                element.ServiceMode = WebsiteElement.StringServiceModes[row["ServiceMode"].ToString()];
                            else
                            {
                                Log.Entry("Plan configuration error: unsupported service mode. Run aborted.");
                                Environment.Exit(1);
                                return;
                            }
                        }
                        if (row["XPATH"] != null && !row.IsNull("XPATH"))
                            element.XPATH = row["XPATH"].ToString();
                        if (row["DataLocation"] != null && !row.IsNull("DataLocation"))
                        {
                            if (WebsiteElement.StringDataLocations.ContainsKey(row["DataLocation"].ToString()))
                                element.DataLocation = WebsiteElement.StringDataLocations[row["DataLocation"].ToString()];
                            else
                            {
                                Log.Entry("Plan configuration error: unsupported data location. Run aborted.");
                                Environment.Exit(1);
                                return;
                            }
                        }                            
                        if (row["SearchElementBeforeLeft"] != null && !row.IsNull("SearchElementBeforeLeft"))
                            element.SearchElementBeforeLeft = row["SearchElementBeforeLeft"].ToString();
                        if (row["SearchElementLeft"] != null && !row.IsNull("SearchElementLeft"))
                            element.SearchElementLeft = row["SearchElementLeft"].ToString();
                        if (row["LeftSEMaxDistance"] != null && !row.IsNull("LeftSEMaxDistance"))
                        {
                            bool s = Int32.TryParse(row["LeftSEMaxDistance"].ToString(), out int m);
                            if (!s)
                            {
                                Log.Entry("Plan configuration error: can't convert LeftSEMaxDistance to int. Run aborted.");
                                Environment.Exit(1);
                                return;
                            }
                            else
                                element.LeftSEMaxDistance = m;
                        }                            
                        if (row["RightSEMaxDistance"] != null && !row.IsNull("RightSEMaxDistance"))
                        {
                            bool s = Int32.TryParse(row["RightSEMaxDistance"].ToString(), out int m);
                            if (!s)
                            {
                                Log.Entry("Plan configuration error: can't convert RightSEMaxDistance to int. Run aborted.");
                                Environment.Exit(1);
                                return;
                            }
                            else
                                element.RightSEMaxDistance = m;
                        }                            
                        if (row["SearchElementRight"] != null && !row.IsNull("SearchElementRight"))
                            element.SearchElementRight = row["SearchElementRight"].ToString();
                        if (row["ConvertingFunction"] != null && !row.IsNull("ConvertingFunction"))
                        {
                            if (StringConverters.StringConvertingFunctions.ContainsKey(row["ConvertingFunction"].ToString()))
                                element.ConvertingFunction = StringConverters.StringConvertingFunctions[row["ConvertingFunction"].ToString()];
                            else
                            {
                                Log.Entry("Plan configuration error: unsupported converting function. Run aborted.");
                                Environment.Exit(1);
                                return;
                            }
                        }                            
                        if (row["ExtraParam"] != null && !row.IsNull("ExtraParam"))
                            element.ExtraParam = row["ExtraParam"].ToString();

                        if (element.ServiceMode == WebsiteElement.ServiceModes.XPATH && (element.XPATH is null || element.XPATH == "")
                            || (element.ServiceMode == WebsiteElement.ServiceModes.DOCTEXT && (element.SearchElementBeforeLeft is null 
                                || element.SearchElementBeforeLeft == "" || element.SearchElementLeft is null || element.SearchElementLeft == "" 
                                || element.SearchElementRight is null || element.SearchElementRight == "" )))
                        {
                            Log.Entry("Plan configuration error: parameters required by service mode are missing (XPATH and data location or search elements and SE distances). Run aborted.");
                            Environment.Exit(1);
                            return;
                        }

                        //....pozostałe 9, testy konwersji i konwersje dla liczb
                        //...dorobić 2 słowniki dla enumów

                            //poniżej - walidacja czy ma nie nullowe pola oraz zestaw nullowych (xpath + data location || search elem x3 + dystanse x2...)


                        structure.WebsiteElements.Add(element);
                    }
                    siteStructure = structure;
                }
                else
                    throw new Exception("RunConfiguration: no rows or wrong number of columns retrieved from MS_CFG_WEBSITE_ELEMENT.");
            }


            string query7 = "SELECT Query FROM MS_CFG_WEBSITE_QUERY WHERE PlanName = '" + planName + "' ORDER BY OrderIndex";
            int rows7 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query7, false, out DataTable? dataTable7);
            if (rows7 == -1 || dataTable7 == null)
            {
                Log.Entry("Failed to read from MS_CFG_WEBSITE_QUERY. Run aborted.");
                Environment.Exit(1);
                return;
            }
            else
            {
                List<string> statements = new();

                if (dataTable7.Rows.Count > 0 && dataTable7.Rows[0].ItemArray.Count() == 1)
                {
                    

                    foreach (DataRow row in dataTable7.Rows)
                    {
                        if (row["Query"] != null && !row.IsNull("Query"))
                            statements.Add(row["Query"].ToString());
                        else
                        {
                            Log.Entry("Empty query in MS_CFG_WEBSITE_QUERY. Run aborted.");
                            Environment.Exit(1);
                            return;
                        }

                    }
                }
                else
                {
                    Log.Entry("No rows returned from MS_CFG_WEBSITE_QUERY or too many columns. Run aborted.");
                    Environment.Exit(1);
                    return;
                }

                siteStructure.SQLStatements = statements;
            }
        }

    }
}


/*
 * cała ta metoda to abominacja, a walidacje sa krzywe i pewnie dziurawe - do przegladu na spokojnie
 * 
 * 
 */