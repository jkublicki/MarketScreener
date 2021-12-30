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
    //założenia: notacja en-US (kropka odziela miejsca dziesiętne, 10^9 to "billion", przecinki są ignorowane)
    internal static class NodeConverters
    {
        const int EvalDecimalPrecisionLimit = 19;

        public enum ConvertingFunctions
        {
            None,
            EvalDecimal,
            EvalInt,
            DecimalRangeLeft,
            DecimalRangeRight,
            Varchar,
            YFMarketCapToMillion,
            GICS
        }


        public static string ConvertValue(string value, ConvertingFunctions converter, out bool success)
        {
            switch (converter)
            {
                case ConvertingFunctions.None:
                    success = true;
                    return value;
                case ConvertingFunctions.EvalDecimal:
                    string v1 = EvalDecimal(value, out bool s1);
                    success = s1;
                    return v1;
                case ConvertingFunctions.EvalInt:
                    string v2 = EvalInt(value, out bool s2);
                    success = s2;
                    return v2; 
                case ConvertingFunctions.DecimalRangeLeft:
                    string v3 = DecimalRangeLeft(value, out bool s3);
                    success = s3;
                    return v3;
                case ConvertingFunctions.DecimalRangeRight:
                    string v4 = DecimalRangeRight(value, out bool s4);
                    success = s4;
                    return v4;
                case ConvertingFunctions.YFMarketCapToMillion:
                    string v5 = YFMarketCapToMillion(value, out bool s5);
                    success = s5;
                    return v5;
                case ConvertingFunctions.GICS:
                    //todo!!
                            success = true;
                            return "'1;1;1'";
                case ConvertingFunctions.Varchar:
                    success = true;
                    return String.Concat("'", value, "'");

                default:
                    success = false;
                    return "NodeConverters.ConvertValue() failed";

            }

        }


        //public static (int, int, int) GICS(string dataPoint, out bool success)
        //zrobić w bazie słownik GICS i korzystać z niego wewnątrz tej metody (ale to podwaja ilość połączeń do bazy, może lepiej mieć GICS tutaj)
        //w takim słowniku powinno być average PE (DM, EM), a to z kolei będzie potrzebne w bazie do oceny PE
        //trzymać słownik w pliku, jak nie ma pliku to sięgnąć do bazy i zrobić plik, może raz na tydzień kasować plik w ramach synchronizacji
        //czytanie pliku co chwilę (co ticker) też jest głupie, trzymać to w zmiennej, jak pusta, załadować z pliku, reszta jw.
        



        private static string YFMarketCapToMillion(string dataPoint, out bool success) //specyficzne dla Yahoo Finance, dotyczy kapitalizacji, short scale
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = "";

            foreach (char c in dataPoint)
            {
                if (Char.IsDigit(c) || c == '.')
                    s += c;
            }

            success = Decimal.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            if (dataPoint.Contains('T'))
                result *= 1000000;
            else if (dataPoint.Contains('B'))
                result *= 1000;            
                        
            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        private static string EvalInt(string dataPoint, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = dataPoint.Trim();

            success = Int32.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out int result);

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        private static string EvalDecimal(string dataPoint, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = dataPoint.Trim();            

            success = Decimal.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            if (s.Length > EvalDecimalPrecisionLimit + 1 && result >= 0 || s.Length > EvalDecimalPrecisionLimit + 2 && result < 0)
            {
                success = false;
                return "0";
            }
            else
                return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));

        }

        
        private static string DecimalRangeLeft(string dataPoint, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
        {
            if (dataPoint == null || dataPoint == "")
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

            success = Decimal.TryParse(s.Split(' ').First(), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        private static string DecimalRangeRight(string dataPoint, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
        {
            if (dataPoint == null || dataPoint == "")
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

            success = Decimal.TryParse(s.Split(' ').Last(), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }
    }
}
