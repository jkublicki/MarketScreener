using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Stores a data value extracted from a website element
namespace MarketScreener.DataHunters.HAP
{
    internal class DataField
    {
        public string Name = "";
        public string Value;
        public bool WebsiteElementFound = false;

        public void Reset()
        {
            Name = "";
            Value = null;
            WebsiteElementFound = false;    
        }
    }
}
