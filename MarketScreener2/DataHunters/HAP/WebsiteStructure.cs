using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal class WebsiteStructure
    {        
        public List<WebsiteElement> WebsiteElements = new List<WebsiteElement>();
        public List<Tuple<string, string>> SQLStatements = new List<Tuple<string, string>>(); //To execute after data extraction

        /*
        public WebsiteStructure(string name)
        {


            WebsiteElements = new List<WebsiteElement>()
            {
                new WebsiteElement()
                {

                    Name = "Price",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[3]/div[1]/div/fin-streamer[1]",
                    DataLocation = WebsiteElement.DataLocations.AttributeValue,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "PriceClose",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[3]/div[1]/div/fin-streamer[1]", //Price = PriceClose, bo obsługuję tylko zamknięte rynki
                    DataLocation = WebsiteElement.DataLocations.AttributeValue,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "PriceOpen",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='OPEN-value']",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "MarketState",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@id='quote-market-notice']/span",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.Varchar50,
                    //ExtraParam = @"open|close" //chcę zapisać całę market state dla sprawdzenia samoobrony YF
                },
                new WebsiteElement()
                {

                    Name = "Currency",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[5]/div/div/div/div[2]/div[1]/div[2]/span",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.Varchar50,
                    ExtraParam = @"([A-z]{3})\s*$" //@ powoduje, że znak specjalny \ jest traktowany jak zwykły; regex do ostatnich 3 znaków
                },
                new WebsiteElement()
                {

                    Name = "CompanyName",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@id='quote-header-info']/div[2]/div[1]/div[1]/h1/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.Varchar50,
                    ExtraParam = @".+?(?= \()"     


                },
                new WebsiteElement()
                {

                    Name = "PreviousClose",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[1]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "TargetEst1y",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    //XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[8]/td[2]", //też działa
                    XPATH = "//*[@data-test='ONE_YEAR_TARGET_PRICE-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "Week52RangeLow",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeLeft,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "Week52RangeHigh",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[6]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeRight,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "DayRangeLow",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[5]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeLeft,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "DayRangeHigh",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[5]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeRight,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "Volume",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[7]/td[2]/fin-streamer",
                    DataLocation = WebsiteElement.DataLocations.InnerHtml,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalInt,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "AvgVolume",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[1]/table/tbody/tr[8]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalInt,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "MarketCapMnUSD",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "/html/body/div[1]/div/div/div[1]/div/div[3]/div[1]/div/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr[1]/td[2]",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.YFMarketCapToMillion,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "Beta",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='BETA_5Y-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal //beta może być ujemna
                },
                new WebsiteElement()
                {

                    Name = "PE",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='PE_RATIO-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal //PE może być ujemne
                },
                new WebsiteElement()
                {

                    Name = "EPSTTM",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='EPS_RATIO-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal
                },
                new WebsiteElement()
                {

                    Name = "EarningsDate",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='EARNINGS_DATE-value']/span/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDate
                },
                new WebsiteElement()
                {

                    Name = "ForwardDividend",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='DIVIDEND_AND_YIELD-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeLeft,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "DividendYield",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='DIVIDEND_AND_YIELD-value']/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.DecimalRangeRight,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "ExDividendDate",
                    ServiceMode = WebsiteElement.ServiceModes.XPATH,
                    XPATH = "//*[@data-test='EX_DIVIDEND_DATE-value']/span/text()",
                    DataLocation = WebsiteElement.DataLocations.InnerText,
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDate
                },

                new WebsiteElement()
                {

                    Name = "GICSSector",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "sector\":\"",
                    SearchElementBeforeLeft = "summaryProfile",
                    LeftSEMaxDistance = 40,
                    RightSEMaxDistance = 40,
                    SearchElementRight = "\",\"",
                    ConvertingFunction = StringConverters.ConvertingFunctions.GICSSector
                },
                new WebsiteElement()
                {

                    Name = "RecommendationRating",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "recommendationMean",
                    LeftSEMaxDistance = 25,
                    RightSEMaxDistance = 25,
                    SearchElementRight = ",\"",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.99"
                },
                new WebsiteElement()
                {

                    Name = "NumberOfAnalystOpinions",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "AnalystOpinions",
                    LeftSEMaxDistance = 20,
                    RightSEMaxDistance = 20,
                    SearchElementRight = ",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalInt,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "ReturnOnAssets",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "returnOnAssets\":", //tu nie może być verbatim string literal, czyli @; YF podaje RoA 5% jako 0.05
                    LeftSEMaxDistance = 20,
                    RightSEMaxDistance = 20,
                    SearchElementRight = ",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal
                },
                new WebsiteElement()
                {

                    Name = "TargetLowPrice",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "targetLowPrice",
                    LeftSEMaxDistance = 20,
                    RightSEMaxDistance = 20,
                    SearchElementRight = ",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "TargetHighPrice",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "targetHighPrice",
                    LeftSEMaxDistance = 20,
                    RightSEMaxDistance = 20,
                    SearchElementRight = ",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                },
                new WebsiteElement()
                {

                    Name = "CountryName",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = ",\"country\":\"",
                    SearchElementBeforeLeft = ",\"phone\":",
                    LeftSEMaxDistance = 40,
                    RightSEMaxDistance = 40,
                    SearchElementRight = "\",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.Varchar50, //dorobić funkcję a la gics mapującą kraje
                    ExtraParam = @"^.{1,25}"
                },
                new WebsiteElement()
                {

                    Name = "PayoutRatio",
                    ServiceMode = WebsiteElement.ServiceModes.DOCTEXT,
                    SearchElementLeft = "raw\":",
                    SearchElementBeforeLeft = "payoutRatio\":{",
                    LeftSEMaxDistance = 20,
                    RightSEMaxDistance = 20,
                    SearchElementRight = ",",
                    ConvertingFunction = StringConverters.ConvertingFunctions.EvalDecimal,
                    ExtraParam = "0.0"
                }
            };

            SQLStatements = new List<string>()
            {
                //na wypadek problemów z pełnym update (np. przekroczna długość pola), aby ticker spadł do części kolejki dla zaktualizowanych dzisiaj
                //bez sensu, nie rozwiązkuje problemu pustego url?
                //ale taki problem nie wystąpi więcej niż raz na jedno pobranie tickerów
                @"IF ((SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}'))) IS NOT NULL)
                    UPDATE ENU_TICKER SET UpdateDate = GETUTCDATE() WHERE TickerYF = '{UrlKey}'",

                //jeżeli rynek jest zamknięty, update ENU_TICKER
                @"IF ((SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}'))) IS NOT NULL)
                    UPDATE ENU_TICKER SET 
                    CompanyName = {CompanyName}
                    ,MarketCapMnUSD = {MarketCapMnUSD}
                    ,GICSSector = {GICSSector}
                    ,Currency = {Currency}
                    ,TradingDay = (SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')))
                    ,Price = {Price}
                    ,PriceClose = {PriceClose}
                    ,PriceOpen = {PriceOpen}
                    ,PreviousClose = {PreviousClose}
                    ,TargetEst1y = {TargetEst1y}
                    ,Week52RangeLow = {Week52RangeLow}
                    ,Week52RangeHigh = {Week52RangeHigh}
                    ,Volume = {Volume}
                    ,AvgVolume = {AvgVolume}
                    ,RecommendationRating = {RecommendationRating}
                    ,Beta = {Beta}
                    ,PE = {PE}
                    ,EPSTTM = {EPSTTM}
                    ,EarningsDate = {EarningsDate}
                    ,ForwardDividend = {ForwardDividend}
                    ,DividendYield = {DividendYield}
                    ,ExDividendDate = {ExDividendDate}
                    ,DayRangeLow = {DayRangeLow}
                    ,DayRangeHigh = {DayRangeHigh}
                    ,CountryName = {CountryName}
                    ,MarketState = {MarketState}
                    ,TargetHighPrice = {TargetHighPrice}
                    ,TargetLowPrice = {TargetLowPrice}
                    ,ReturnOnAssets = {ReturnOnAssets}
                    ,NumberOfAnalystOpinions = {NumberOfAnalystOpinions}
                    ,PayoutRatio = {PayoutRatio}
                    ,UpdateDate = GETUTCDATE()
                    WHERE TickerYF = '{UrlKey}'",

                //jeżeli rynek jest zamknięty i istnieje rekord TICKER_HISTORY, update TICKER_HISTORY, w przeciwnym wypadku insert
                @"IF ((SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}'))) IS NOT NULL)
                BEGIN
                    IF (EXISTS (SELECT 1 FROM TICKER_HISTORY WHERE TickerGF = (SELECT TOP 1 TickerGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}') 
                            AND TradingDay = (SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')))))
                        UPDATE TICKER_HISTORY SET 
                        Price = {Price}
                        ,PriceOpen = {PriceOpen}
                        ,PriceClose = {PriceClose}
                        ,PreviousClose = {PreviousClose}
                        ,TargetEst1y = {TargetEst1y}
                        ,MarketState = {MarketState}
                        ,Week52RangeLow = {Week52RangeLow}
                        ,Week52RangeHigh = {Week52RangeHigh}
                        ,Volume = {Volume}
                        ,AvgVolume = {AvgVolume}
                        ,RecommendationRating = {RecommendationRating}
                        ,Beta = {Beta}
                        ,PE = {PE}
                        ,EPSTTM = {EPSTTM}
                        ,EarningsDate = {EarningsDate}
                        ,ForwardDividend = {ForwardDividend}
                        ,DividendYield = {DividendYield}
                        ,ExDividendDate = {ExDividendDate}
                        ,DayRangeLow = {DayRangeLow}
                        ,DayRangeHigh = {DayRangeHigh}
                        ,TargetHighPrice = {TargetHighPrice}
                        ,TargetLowPrice = {TargetLowPrice}
                        ,ReturnOnAssets = {ReturnOnAssets}
                        ,NumberOfAnalystOpinions = {NumberOfAnalystOpinions}
                        ,PayoutRatio = {PayoutRatio}
                        ,UpdateDate = GETUTCDATE()
                        WHERE TickerGF = (SELECT TOP 1 TickerGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')
                            AND TradingDay = (SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')))
                    ELSE
                        INSERT INTO TICKER_HISTORY 
                        (Price
                        ,PriceOpen
                        ,PriceClose
                        ,MarketState
                        ,PreviousClose
                        ,TargetEst1y
                        ,Week52RangeLow
                        ,Week52RangeHigh
                        ,DayRangeLow
                        ,DayRangeHigh
                        ,Volume
                        ,AvgVolume
                        ,MarketCapMnUSD
                        ,Beta
                        ,PE
                        ,EPSTTM
                        ,EarningsDate
                        ,ForwardDividend
                        ,DividendYield
                        ,ExDividendDate
                        ,RecommendationRating
                        ,NumberOfAnalystOpinions
                        ,ReturnOnAssets
                        ,TargetLowPrice
                        ,TargetHighPrice
                        ,PayoutRatio
                        ,UpdateDate
                        ,TradingDay
                        ,TickerGF)
                        VALUES 
                        ({Price}
                        ,{PriceOpen}
                        ,{PriceClose}
                        ,{MarketState}
                        ,{PreviousClose}
                        ,{TargetEst1y}
                        ,{Week52RangeLow}
                        ,{Week52RangeHigh}
                        ,{DayRangeLow}
                        ,{DayRangeHigh}
                        ,{Volume}
                        ,{AvgVolume}
                        ,{MarketCapMnUSD}
                        ,{Beta}
                        ,{PE}
                        ,{EPSTTM}
                        ,{EarningsDate}
                        ,{ForwardDividend}
                        ,{DividendYield}
                        ,{ExDividendDate}
                        ,{RecommendationRating}
                        ,{NumberOfAnalystOpinions}
                        ,{ReturnOnAssets}
                        ,{TargetLowPrice}
                        ,{TargetHighPrice}
                        ,{PayoutRatio}
                        ,GETUTCDATE() 
                        ,(SELECT dbo.ReadTimeToTradingDay (GETUTCDATE(), (SELECT TOP 1 MarketCodeGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')))
                        ,(SELECT TOP 1 TickerGF FROM ENU_TICKER WHERE TickerYF = '{UrlKey}')
                        )
                END"
            };




        }
        */
    }
}
