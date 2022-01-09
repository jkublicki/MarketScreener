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
                if (siteStructure == null)
                {
                    siteStructure = new("dupa_123");
                    return siteStructure;
                }
                    
                else
                    return siteStructure;
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
            string query1 = "SELECT Count(1) FROM MS_CFG_PLAN WHERE IsActivePlan = 1";
            int rows1 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query1, false, out DataTable? dataTable1);
            if (rows1 == -1 || dataTable1 == null)
            {
                Log.Entry("Failed to count records in MS_CFG_PLAN. Run aborted.");
                Environment.Exit(1);
                return;
            }
            if (dataTable1 != null && dataTable1.Rows.Count > 1)
            {
                Log.Entry("Plan configuration error: more than one row in MS_CFG_PLAN has IsActivePlan = 1. Only one active plan is allowed. Run aborted.");
                Environment.Exit(1); //powinien być enum z exit codami
                return;
            }
            if (dataTable1 != null && dataTable1.Rows.Count == 0)
            {
                Log.Entry("Plan configuration error: there is no row in MS_CFG_PLAN that has IsActivePlan = 1. One active plan is required. Run aborted.");
                Environment.Exit(1);
                return;
            }

            string query2 = "SELECT TOP 1 PlanName, RunDurationH, UrlSetName FROM MS_CFG_PLAN WHERE IsActivePlan = 1";
            int rows2 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query2, false, out DataTable? dataTable2);
            if (rows2 == -1 || dataTable2 == null)
            {
                Log.Entry("Failed to read plan from MS_CFG_PLAN. Run aborted.");
                Environment.Exit(1);
                return;
            }
            else
            {
                if (dataTable2.Rows[0] != null && dataTable2.Rows[0].ItemArray.Count() == 3)
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
            }

            //docelowo analogicznie jw. dla website structure i website elements



        }

    }
}
