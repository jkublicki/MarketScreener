using MarketScreener.DataHunters.HAP;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Protocols.WSTrust;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace MarketScreener
{
    internal class WebsiteDownloader
    {
        private HttpClient httpClient;

        private class Header
        {
            public Header(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name;
            public string Value;
        }

        private class HeaderSet
        {
            public HeaderSet()
            {
                Headers = new List<Header>();
            }

            public List<Header> Headers;
        }

        private List<HeaderSet> headerSets = new List<HeaderSet>();

        public WebsiteDownloader()
        {
            ReadFromDatabase();

            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            httpClient = new HttpClient(handler);

        }

        //zainicjowac w HapManager.MarketScreenerRun(), tak samo jak PlanConfiguation
        //w inicjalizacji pobrac ustawienia z bazy: headers itp.


        private void ReadFromDatabase()
        {
            List<string> headerSetIds = new List<string>();

            string query1 = "SELECT Distinct HeaderSet FROM MS_CFG_ENU_HEADER WHERE IsEnabled = 1";
            int rows1 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query1, false, out DataTable dataTable1);
            if (rows1 == -1 || dataTable1 == null || dataTable1.Rows.Count == 0 || dataTable1.Rows[0].ItemArray.Length == 0 || dataTable1.Rows[0].IsNull(0))
            {
                Log.Entry("Failed to find enabled headers in MS_CFG_ENU_HEADER. Run aborted.");
                Environment.Exit(1);
                return;
            }
            else
            {
                foreach (DataRow row in dataTable1.Rows)
                {
                    if (row["HeaderSet"] != null && !row.IsNull("HeaderSet"))
                    {
                        headerSetIds.Add(row["HeaderSet"].ToString());
                    }
                }
            }

            foreach (string setId in headerSetIds)
            {
                string query2 = "SELECT Name, Value FROM MS_CFG_ENU_HEADER WHERE IsEnabled = 1 AND HeaderSet = " + setId;
                int rows2 = QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query2, false, out DataTable dataTable2);
                if (rows2 == -1 || dataTable2 == null || dataTable2.Rows.Count == 0 || dataTable2.Rows[0].ItemArray.Length == 0 || dataTable2.Rows[0].IsNull(0))
                {
                    Log.Entry("Problem reading MS_CFG_ENU_HEADER. Run aborted.");
                    Environment.Exit(1);
                    return;
                }
                else
                {
                    HeaderSet headerSet = new HeaderSet();

                    foreach (DataRow row in dataTable2.Rows)
                    {
                        if (row["Name"] != null && !row.IsNull("Name") && row["Value"] != null && !row.IsNull("Value"))
                        {
                            Header h = new Header(row["Name"].ToString(), row["Value"].ToString());
                            headerSet.Headers.Add(h);
                        }
                    }

                    headerSets.Add(headerSet);  
                }
            }
        }

        
        public string DownloadWebsite(string url, out bool IsSuccessful)
        {
            try
            {   
                httpClient.DefaultRequestHeaders.Clear();

                var random = new Random();
                var randomHeaderSet = headerSets[random.Next(headerSets.Count)];
                foreach (var header in randomHeaderSet.Headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);
                }

                var response = httpClient.GetAsync(url).Result;

                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    if (HAPSettings.DebugEnabled)
                    {
                        Log.Entry("WebsiteDownloader: servicing response encoded as gzip.");
                    }

                    using (var responseStream = response.Content.ReadAsStreamAsync().Result)
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressedStream))
                    {
                        IsSuccessful = true;
                        return reader.ReadToEnd();
                    }
                }
                else
                {
                    IsSuccessful = true;
                    return response.Content.ReadAsStringAsync().Result;
                }

                //response.EnsureSuccessStatusCode(); // Ensure success status code               
                
            }
            catch (Exception e)
            {
                if (HAPSettings.LogEnabled) Log.Entry(String.Concat("WebsiteDownloader failed for url ", url,
                    ". Full exception: ", e.ToString(), ""));
                IsSuccessful = false;
                return null;
            }
        }
    }




}
