using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal class HAPDiagnostics
    {
        public int DeadUrlCount = 0;
        public List<(string, string)> DeadUrls = new List<(string, string)>();
        public int FailedUrlLoadsCount = 0;
        public List<(string, string)> FailedUrlLoads = new List<(string, string)>();
        public int FailedSQLStatementsCount = 0;
        public List<(string, DateTime)> FailedSQLStatements = new List<(string, DateTime)>();
        public int BrokenWebsitesCount = 0;
        public List<(string, string)> BrokenWebsites = new List<(string, string)>();
        public int FindElementFailsCount = 0;
        public int ExtractDataFailsCount = 0;
        public int FailedConversionsCount = 0;
        public List<(string, StringConverters.ConvertingFunctions)> FailedConversions = new List<(string, StringConverters.ConvertingFunctions)>();
        public int TechnicalFailsCount = 0;

        public int LoadedUrlsCount = 0;
        public int ExecutedSQLStatementsCount = 0;
        public int ElementsFoundCount = 0;
        public int ExtractedDataCount = 0;
        public int ExecutedConversionsCount = 0;

        private DateTime startTime = DateTime.UtcNow;
        public TimeSpan TimeElapsed { get => DateTime.UtcNow - startTime; }
        public decimal Health { 
            get => (DeadUrlCount + FailedUrlLoadsCount + LoadedUrlsCount) >= 10
                && (DeadUrlCount + FailedUrlLoadsCount + LoadedUrlsCount) > 0
                && (FindElementFailsCount + ElementsFoundCount) > 0
                && (FailedSQLStatementsCount + ExecutedSQLStatementsCount) > 0
                ? 
                ((decimal)LoadedUrlsCount / (DeadUrlCount + FailedUrlLoadsCount + LoadedUrlsCount))
                * ((decimal)ExecutedConversionsCount / (FindElementFailsCount + ElementsFoundCount))
                * ((decimal)ExecutedSQLStatementsCount / (FailedSQLStatementsCount + ExecutedSQLStatementsCount))
                : -1; 
        }
        public decimal Speed { get => (decimal)TimeElapsed.TotalMinutes > 0 ? Math.Round((DeadUrlCount + FailedUrlLoadsCount + LoadedUrlsCount) / (decimal)TimeElapsed.TotalMinutes, 2) : -1; }

        public string Print()
        {
            return String.Concat("Dead urls: ", DeadUrlCount.ToString(), 
                "\nWebsite load fails: ", FailedUrlLoadsCount.ToString(),
                "\nWebsites loaded: ", LoadedUrlsCount.ToString(),
                "\nBroken websites: ", BrokenWebsitesCount.ToString(),
                "\nTechnical fails: ", TechnicalFailsCount.ToString(),                
                "\nFind element fails: ", FindElementFailsCount.ToString(),
                "\nElements found: ", ElementsFoundCount.ToString(),
                "\nData extraction fails: ", ExtractDataFailsCount.ToString(),
                "\nData extracted: ", ExtractedDataCount.ToString(),
                "\nData conversion fails: ", FailedConversionsCount.ToString(),
                "\nSuccessful data conversions: ", ExecutedConversionsCount.ToString(),
                "\nFailed SQL statements: ", FailedSQLStatementsCount.ToString(),
                "\nExecuted SQL statements: ", ExecutedSQLStatementsCount.ToString(),
                "\nTime elapsed: ", TimeElapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
                "\nSpeed: ", Speed.ToString(), " urls/min",
                (Health != -1 ? ("\nHealth: " + Math.Round(100 * Health, 1).ToString() + "%") : "\nHealth: N/A"),
                "\n");
        }
    }
}
