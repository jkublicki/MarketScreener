using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal static class HAPxYFSettings
    {
        public static WebsiteNodes.WebsiteNodeSet YahooEquityNodeSet()
        {
            WebsiteNodes.WebsiteNodeSet yahooEquityNodeSet = new()
            {
                Website = "finance.yahoo.com",
                WebsiteType = "Equity",
                UpdateDate = DateTime.Now,
                Nodes = new List<WebsiteNodes.WebsiteNode>()
                {
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Price",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[3]/div[1]/div/fin-streamer[1]",
                        DataLocation = WebsiteNodes.DataLocations.AttributeValue,
                        ColumnName = "Price",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Decimal
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "PreviousClose",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[1]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "PreviousClose",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Decimal
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "1yTargetEst",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[8]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "1TargetEst",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Decimal
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "52WeekRangeLow",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "52WeekRangeLow",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.DecimalRangeLeft
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "52WeekRangeHigh",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "52WeekRangeHigh",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.DecimalRangeRight
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Volume",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[7]/td[2]/fin-streamer",
                        DataLocation = WebsiteNodes.DataLocations.InnerHtml,
                        ColumnName = "Volume",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Int
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "AvgVolume",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[8]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "AvgVolume",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Int
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Sector",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "sector\":\"",
                        SearchElementBeforeLeft = "summaryProfile",
                        LeftSEMaxDistance = 40,
                        SearchElementRight = "\",\"",
                        ColumnName = "Sector",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" }
                        //jaka konwersja? nie chcę opisywać tekstowo sektorów, aby oszczędzać miejsce w bazie
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "RecommendationRating",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "recommendationMean",
                        LeftSEMaxDistance = 10,
                        SearchElementRight = ",\"",
                        ColumnName = "Sector",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = NodeConverters.ConvertingFunctions.Decimal
                    }
                }
            };

            return yahooEquityNodeSet;
        }
    }
}
