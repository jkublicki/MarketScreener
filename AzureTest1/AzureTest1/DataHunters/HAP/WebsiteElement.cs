using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Stores informations required for data extraction
namespace MarketScreener.DataHunters.HAP
{
    internal class WebsiteElement
    {
        public string Name = "";
        public ServiceModes ServiceMode; //określa metodę szukania danych: XPATH, DOCTEXT
        public string? XPATH;
        public DataLocations? DataLocation; //określa położenie danych w node, tylko dla XPATH: AttributeValue, InnerText, InnerHtml            
        public string? SearchElementBeforeLeft; //jn. poprzedzający SearchElementLeft
        public string? SearchElementLeft; //charakterystyczny stały tekst poprzedzający dane, tylko dla DOCTEXT
        public int? LeftSEMaxDistance;
        public int? RightSEMaxDistance;
        public string? SearchElementRight; //jw. występujący po danych
        public StringConverters.ConvertingFunctions ConverterFunction;
        public string? ExtraParam; //parametr, zastosowanie specyficzne dla konwertera: varchar - regex, liczbowe - dolny limit (nieakceptowana wartość)


        public enum ServiceModes
        {
            XPATH,
            DOCTEXT
        }

        public enum DataLocations
        {
            AttributeValue,
            InnerText,
            InnerHtml
        }
    }
}
