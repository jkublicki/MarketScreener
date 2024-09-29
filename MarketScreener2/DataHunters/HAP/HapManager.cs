using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal class HAPManager
    {
        private PlanConfiguration planConfiguration = new PlanConfiguration();
        private WebsiteDownloader websiteDownloader = new WebsiteDownloader();
           

        private List<(string, string)> _urls;
        private List<(string, string)> urls
        {
            get
            {                
                if (_urls == null)
                {
                    _urls = new List<(string, string)>();

                    if (HAPSettings.DebugEnabled && HAPSettings.TestUrl != null) //tryb testowy
                    {
                        _urls.Add(((string, string))HAPSettings.TestUrl);
                        if (HAPSettings.LogEnabled)
                            Log.Entry(String.Concat("Running test url\n"));
                        return _urls;
                    }

                    string query = String.Concat("SELECT UrlKey, Url FROM ", planConfiguration.UrlSetName);
                    int rows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable dataTable);
                    if (rows == -1 || dataTable == null)
                    {
                        Log.Entry(String.Concat("Can't read from url set ", planConfiguration.UrlSetName));
                    }
                    else
                    {
                        if (HAPSettings.LogEnabled) 
                            Log.Entry(String.Concat(dataTable.Rows.Count.ToString(), " rows retrieved from url set ", planConfiguration.UrlSetName, ".\n"));

                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (!row.IsNull(0) && !row.IsNull(1))
                                _urls.Add((row.ItemArray[0].ToString(), row.ItemArray[1].ToString()));  
                            else
                                Log.Entry(String.Concat("Configuration error: some rows returned from url set ", planConfiguration.UrlSetName, " contain null values.\n"));
                        }
                    }
                        return _urls;
                }
                else
                    return _urls;
            }
        }

        private bool breakSingal = false;
        HAPDiagnostics diag = new HAPDiagnostics();

        private void Service()
        {


            if (urls.Any())
            {
                (string, string) url = urls.First();
                urls.Remove(url);
                
                new HAPDataExtractor().Extract(url.Item1, url.Item2, planConfiguration.SiteStructure, websiteDownloader, ref diag);
            }
        }

        public void MarketScreenerRun()
        {
            DateTime runStartTime = DateTime.UtcNow;
            DateTime lastServiceEndTime = DateTime.UtcNow.AddHours(-1);
            int waitTimeMs = 5000;

            string consoleMessage = "MarketScreener will be running " + planConfiguration.PlanName + " for " + planConfiguration.RunDurationMin.ToString() + " minutes from " + runStartTime.ToString("yyyy-MM-dd HH:mm") + " UTC.\n";
            if (HAPSettings.DebugEnabled)
                consoleMessage += "Debug enabled\n" + HAPSettings.Print();
            
            if (urls.Count > 0)
                consoleMessage += String.Concat("Keys in url set (", urls.Count.ToString(), "): ", 
                    urls.Count > 5 ? String.Join(", ", urls.Take(3).Select(e => e.Item1)) + ", ..., " + urls.Last().Item1 : String.Join(", ", urls.Select(e => e.Item1)), "\n");
            
            Log.Entry(consoleMessage);

            int count = 0;

            while (urls.Any() && !breakSingal && (DateTime.UtcNow - runStartTime).TotalMinutes < planConfiguration.RunDurationMin)
            {
                if ((DateTime.UtcNow - lastServiceEndTime).TotalMilliseconds > waitTimeMs)
                {
                    //Service(); - tak było oryginalnie, początek snu na koniec service - zmieniam 2022.01.16 21:55 utc, będzie początek snu równocześnie z service - chcę większej prędkości i większej kontroli nad prędkością

                    //dla 1667 odpala się mechanizm obronny YF
                    //dla 3334 * (R * 2.0 + 1) YF się nie bronił, próbka 88 stron; bronił po próbce 157 stron
                    //dla 3334 * (R * 0.5 + 1) YF się bronił, próbka 41
                    //dla 2700 * (R * 2.0 + 1) YF się bronił, próbka 94
                    //dla 2500 * { 10%: R * 2 + 20, 90%: R * 2 + 1 YF się bronił, próbka 77
                    //dla 3000 * { 15%: R * 2 + 20, 85%: R * 2 + 1 YF się nie bronił dla próbki 201; <= TO JEST OK DLA YF, NIE RUSZAĆ!

                    double b = HAPSettings.DelayBase;
                    double c = HAPSettings.LongDelayChance;
                    double mu = HAPSettings.DelayRandomMul;
                    double mo = HAPSettings.LongDelayRandomMod;

                    Random r = new Random();

                    waitTimeMs = (int)Math.Floor(b * (r.NextDouble() > c ? (r.NextDouble() * mu + 1) : (r.NextDouble() * mu + mo)));

                    lastServiceEndTime = DateTime.UtcNow;


                    Service(); //po zmianie 2022.01.16 21:55 utc, pilnować nienaturalnie nieświeżych stron / obrony YF


                    count++;
                    if (count % 5 == 0) //właściwe jest 8? im dłużej, tym gorzej przy sql fail, ale nie przerwie wpisywania STOP
                    {
                        Log.Entry(consoleMessage + "\nDiagnostics (" + DateTime.UtcNow.ToString("HH:mm:ss") + "):\n" + diag.Print());
                    }
                        
                }            
                
            }

            //odtworzenie paździerza 2024, bo trigger
            //to nie działa, query się wywala
            string query = "INSERT INTO AZURE_FUNCTION_STATUS (Timestamp, Message, LogHistory) VALUES (GETUTCDATE(), 'MS Function1 finished', LEFT('"
                + "<style>body {font-family: monospace}</style><br>" + Log.LogHistory.Replace("'", "^").Replace("\n", "<br>") + "', 7000))";
            QueryDatabase.ExecuteSQLStatementNQ(Secrets.ConnectionString, new List<string> { query }, false);
            
        }

        public void BreakRun()
        {
            breakSingal = true;
        }

    }
}
