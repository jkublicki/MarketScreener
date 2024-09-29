using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MarketScreener
{
    public class MarketScreener
    {
        //odkomentowaæ
        private static DataHunters.HAP.HAPManager manager;

        private readonly ILogger _logger;

        public MarketScreener(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MarketScreener>();
            Log.Logger = _logger;
            Log.ClearHistory();
        }

        [Function("MarketScreener")]
        public void Run([TimerTrigger("0 */10 * * * *")] MyInfo myTimer) //default: "0 */10 * * * *"
        {
            Log.Entry("MarketScreener.Run(): C# Timer trigger function executed at: " + DateTime.Now.ToString() + "\n");

            manager = new DataHunters.HAP.HAPManager();
            manager.MarketScreenerRun();
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
