using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAPxYahooFinance
{
    internal class WebsiteNodes
    {
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


        internal class WebsiteNode
        {
            public string Website;
            public string Ticker;
            public string Name;
            public ServiceModes? ServiceMode; //określa metodę szukania danych: XPATH, DOCTEXT
            public string FullXPATH;
            public DataLocations DataLocation; //określa położenie danych w node, tylko dla XPATH: AttributeValue, InnerText, InnerHtml            
            public string SearchElementBeforeLeft; //jn. poprzedzający SearchElementLeft
            public string SearchElementLeft; //charakterystyczny stały tekst poprzedzający dane, tylko dla DOCTEXT
            public int LeftSEMaxDistance;
            public string SearchElementRight; //jw. występujący po danych
            public string? Value; //może default value @"NULL"
            public NodeConverters.ConvertingFunctions ConverterFunction;
            public string ColumnName;
            public List<string> Tables; //zakładając, że update wielu tabel, ale tabele mają tak samo nazwane kolumny odpowiadające temu punktowi danych
            public string? ExtraParam; //parametr, zastosowanie specyficzne dla konwertera: varchar - regex, liczbowe - dolny limit (nieakceptowany)
            public bool DataAreaFound = false;
        }

        internal class WebsiteNodeSet
        {
            public string Website;
            public string WebsiteType;
            public string Ticker;
            public DateTime UpdateDate;
            public List<WebsiteNode> Nodes;
        }
    }
}
