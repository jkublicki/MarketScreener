using Microsoft.Extensions.Logging;

namespace MarketScreener
{
    public static class Log
    {
        private static string logHistory = "";
        
        public static ILogger Logger //wymaga ustawienia przez klasę, która ma logger
        {
            get; set;
        }

        public static void Entry(string entry)
        {
            Logger.LogWarning(entry);
            logHistory += entry;    
        }

        public static string LogHistory
        { 
            get 
            { 
                return logHistory; 
            } 
        }

        public static void ClearHistory()
        {
            logHistory = "";
        }

    }
}
