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
        private PlanConfiguration planConfiguration = new();

        private List<(string, string)>? _urls;
        private List<(string, string)> urls
        {
            get
            {
                if (_urls == null)
                {
                    _urls = new();
                    string query = String.Concat("SELECT UrlKey, Url FROM ", planConfiguration.UrlSetName);
                    int rows = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable? dataTable);
                    if (rows == -1 || dataTable == null)
                    {
                        Log.Entry(String.Concat("Can't read from url set ", planConfiguration.UrlSetName));
                    }
                    else
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (!row.IsNull(0) && !row.IsNull(1))
                                _urls.Add((row.ItemArray[0].ToString(), row.ItemArray[1].ToString()));  
                            else
                                Log.Entry(String.Concat("Configuration error: some rows returned from url set ", planConfiguration.UrlSetName, " contain null values."));
                        }
                    }
                        return _urls;
                }
                else
                    return _urls;
            }
        }

        private bool breakSingal = false;

        private void Service()
        {
            if (urls.Any())
            {
                (string, string) url = urls.First();
                urls.Remove(url);

                new HAP.HAPDataExtractor().Extract(url.Item1, url.Item2, planConfiguration.SiteStructure);
            }
        }

        public void Run()
        {
            //tempshit, dodać timer
            while (urls.Any() && !breakSingal)
            {
                Service();
                Thread.Sleep((int)Math.Floor(2000 * (decimal)(new Random().NextDouble() * 2.0 + 1)));
            }
        }

        public void BreakRun()
        {
            breakSingal = true;
        }

        //TODO po 2022-01-09
        //publiczna funkcja Run()
        //w niej dla jednego z urls new HAP.HAPDataExtractor().Extract(..., ..., planConfigurtion.SiteStructure); 
        //używa pierwszego url do góry i usuwa go z listy
        //potem czeka a pomocą timera losowy czas, ok 1-4 s
        //loguje początek i koniec run
        //wymyślić jak zrobić manualne przerwanie, aby było pomiędzy extractami - nasłuch na event w Main()?
        //docelowo pobierać website structure z bazy - rozbudować PlanConfiguration.cs
        //czy ok że ten manager nie ma konstruktora??
        //z innej beczki - niech wywołanie tej aplikacji będzie co godzine na niecala godzine - na wypadek problemu z polaczeniem na etapie pobierania configa, bo to moze udupic całe działanie

    }
}
