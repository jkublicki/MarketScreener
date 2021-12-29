using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace MarketScreener.DataHunters.HAPxYahooFinance
{

    //funkcje konwertujące odczytane dane na string nadający się do SQL-a, ewaluacja odczytanych danych
    internal static class NodeConverters
    {
        public enum ConvertingFunctions
        {
            None,
            Decimal,
            Int,
            DecimalRangeLeft,
            DecimalRangeRight,
            BillionToMillion,
            GICS
        }

        
        public static string DecimalRangeLeft(string dataPoint, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
        {
            if (dataPoint == null)
            {
                success = true;
                return "NULL";
            }

            string s = "";
            foreach (char c in dataPoint)
            {
                if (Char.IsDigit(c) || c == '.')
                    s += c;
                else if (c != ',')
                    s += ' ';
            }

            Debug.WriteLine(s);
            Debug.WriteLine(s.Split(' ').First());

            success = Decimal.TryParse(s.Split(' ').First(), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            Debug.WriteLine(success.ToString() + ", " + result.ToString(CultureInfo.CreateSpecificCulture("en-US")));

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static string DecimalRangeRight(string dataPoint, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
        {
            if (dataPoint == null)
            {
                success = true;
                return "NULL";
            }

            string s = "";
            foreach (char c in dataPoint)
            {
                if (Char.IsDigit(c) || c == '.')
                    s += c;
                else if (c != ',')
                    s += ' ';
            }

            Debug.WriteLine(s);
            Debug.WriteLine(s.Split(' ').Last());

            success = Decimal.TryParse(s.Split(' ').Last(), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            Debug.WriteLine(success.ToString() + ", " + result.ToString(CultureInfo.CreateSpecificCulture("en-US")));

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }
    }
}
