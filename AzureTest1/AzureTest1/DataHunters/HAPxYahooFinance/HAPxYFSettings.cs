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
                UpdateDate = DateTime.UtcNow,
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
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "PriceOpen",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='OPEN-value']",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "PriceOpen",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "MarketState",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@id='quote-market-notice']/span",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "MarketState",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.Varchar50,
                        ExtraParam = @"open|close" //@ powoduje, że znak specjalny \ jest traktowany jak zwykły; regex do ostatnich 3 znaków
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Currency",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[2]/div[1]/div[2]/span",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "Currency",
                        Tables = new List<string>() { "ENU_TICKER" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.Varchar50,
                        ExtraParam = @"([A-z]{3})\s*$" //@ powoduje, że znak specjalny \ jest traktowany jak zwykły; regex do ostatnich 3 znaków
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "CompanyName",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@id='quote-header-info']/div[2]/div[1]/div[1]/h1/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "CompanyName",
                        Tables = new List<string>() { "ENU_TICKER" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.Varchar50,
                        ExtraParam = @".+?(?= \()"
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
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "TargetEst1y",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        //FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[8]/td[2]", //też działa
                        FullXPATH = "//*[@data-test='ONE_YEAR_TARGET_PRICE-value']/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "TargetEst1y",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Week52RangeLow",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "Week52RangeLow",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeLeft,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Week52RangeHigh",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "Week52RangeHigh",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeRight,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "DayRangeLow",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[5]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "DayRangeLow",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeLeft,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "DayRangeHigh",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[5]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "DayRangeHigh",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeRight,
                        ExtraParam = "0.0"
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
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalInt,
                        ExtraParam = "0.0"
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
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalInt,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "MarketCapMnUSD",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[1]/td[2]",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "MarketCapMnUSD",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.YFMarketCapToMillion,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "Beta",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='BETA_5Y-value']/text()", 
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "Beta",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal //beta może być ujemna
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "PE",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='PE_RATIO-value']/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "PE",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal //PE może być ujemne
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "EPSTTM",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='EPS_RATIO-value']/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "EPSTTM",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "EarningsDate",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='EARNINGS_DATE-value']/span/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "EarningsDate",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDate
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "ForwardDividend",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='DIVIDEND_AND_YIELD-value']/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "ForwardDividend",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeLeft,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "DividendYield",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='DIVIDEND_AND_YIELD-value']/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "DividendYield",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.DecimalRangeRight,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "ExDividendDate",
                        ServiceMode = WebsiteNodes.ServiceModes.XPATH,
                        FullXPATH = "//*[@data-test='EX_DIVIDEND_DATE-value']/span/text()",
                        DataLocation = WebsiteNodes.DataLocations.InnerText,
                        ColumnName = "ExDividendDate",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDate
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
                        ColumnName = "GICSSector",
                        Tables = new List<string>() { "ENU_TICKER" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.GICSSector
                    },                    
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "RecommendationRating",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "recommendationMean",
                        LeftSEMaxDistance = 25,
                        SearchElementRight = ",\"",
                        ColumnName = "RecommendationRating", //rec-rating-txt
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.99"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "NumberOfAnalystOpinions",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "AnalystOpinions",
                        LeftSEMaxDistance = 20,
                        SearchElementRight = ",",
                        ColumnName = "NumberOfAnalystOpinions", //rec-rating-txt
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalInt,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "ReturnOnAssets",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "returnOnAssets\":", //tu nie może być verbatim string literal, czyli @; YF podaje RoA 5% jako 0.05
                        LeftSEMaxDistance = 20,
                        SearchElementRight = ",",
                        ColumnName = "ReturnOnAssets", 
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal
                    },                    
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "TargetLowPrice",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "targetLowPrice",
                        LeftSEMaxDistance = 20,
                        SearchElementRight = ",",
                        ColumnName = "TargetLowPrice", 
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "TargetHighPrice",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "targetHighPrice",
                        LeftSEMaxDistance = 20,
                        SearchElementRight = ",",
                        ColumnName = "TargetHighPrice", 
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "CountryName",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = ",\"country\":\"",
                        SearchElementBeforeLeft = ",\"phone\":",
                        LeftSEMaxDistance = 40,
                        SearchElementRight = "\",",
                        ColumnName = "CountryName",
                        Tables = new List<string>() { "ENU_TICKER" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.Varchar50, //dorobić funkcję a la gics mapującą kraje
                        ExtraParam = @"^.{1,25}"
                    },
                    new WebsiteNodes.WebsiteNode()
                    {
                        Website = "finance.yahoo.com",
                        Name = "PayoutRatio",
                        ServiceMode = WebsiteNodes.ServiceModes.DOCTEXT,
                        SearchElementLeft = "raw\":",
                        SearchElementBeforeLeft = "payoutRatio\":{",
                        LeftSEMaxDistance = 20,
                        SearchElementRight = ",",
                        ColumnName = "PayoutRatio",
                        Tables = new List<string>() { "ENU_TICKER", "TICKER_HISTORY" },
                        ConverterFunction = HAP.StringConverters.ConvertingFunctions.EvalDecimal,
                        ExtraParam = "0.0"
                    }
                }
            };

            return yahooEquityNodeSet;
        }
    }
}
