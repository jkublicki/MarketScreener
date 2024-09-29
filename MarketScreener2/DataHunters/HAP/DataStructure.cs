using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.DataHunters.HAP
{
    internal class DataStructure
    {
        public string UrlKey = ""; //informacja o zadanym url, np. "TROW" dla "https://finance.yahoo.com/quote/TROW?p=TROW"; potrzebne do skręcenia SQL-a, bo zdefiniowany w scenariuszu SQL może zawierać tylko odniesienia do DataStructure
        public List<DataField> DataFields = new List<DataField>();
    }
}
