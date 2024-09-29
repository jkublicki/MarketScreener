using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace MarketScreener.DataHunters.HAP
{

    //funkcje konwertujące odczytane dane na string nadający się do SQL-a, ewaluacja odczytanych danych
    //założenia: notacja en-US (kropka odziela miejsca dziesiętne, 10^9 to "billion", przecinki są ignorowane)
    internal static class StringConverters
    {
        const int EvalDecimalPrecisionLimit = 19;

        public enum ConvertingFunctions
        {
            EvalDecimal,
            EvalInt,
            DecimalRangeLeft,
            DecimalRangeRight,
            Varchar50,
            Varchar,
            EvalDate,
            YFMarketCapToMillion,
            GICSSector
        }

        public static Dictionary<string, ConvertingFunctions> StringConvertingFunctions = new Dictionary<string, ConvertingFunctions>()
        {
            { "EvalDecimal", ConvertingFunctions.EvalDecimal },
            { "EvalInt", ConvertingFunctions.EvalInt },
            { "DecimalRangeLeft", ConvertingFunctions.DecimalRangeLeft },
            { "DecimalRangeRight", ConvertingFunctions.DecimalRangeRight },
            { "Varchar50", ConvertingFunctions.Varchar50 },
            { "Varchar", ConvertingFunctions.Varchar },
            { "EvalDate", ConvertingFunctions.EvalDate },
            { "YFMarketCapToMillion", ConvertingFunctions.YFMarketCapToMillion },
            { "GICSSector", ConvertingFunctions.GICSSector }
        };


        public static string ConvertValue(string value, ConvertingFunctions converter, string extraParam, out bool success)
        {
            switch (converter)
            {
                case ConvertingFunctions.EvalDecimal:
                    string v1 = EvalDecimal(value, extraParam, out bool s1);
                    success = s1;
                    return v1;
                case ConvertingFunctions.EvalInt:
                    string v2 = EvalInt(value, extraParam, out bool s2);
                    success = s2;
                    return v2; 
                case ConvertingFunctions.DecimalRangeLeft:
                    string v3 = DecimalRangeLeft(value, extraParam, out bool s3);
                    success = s3;
                    return v3;
                case ConvertingFunctions.DecimalRangeRight:
                    string v4 = DecimalRangeRight(value, extraParam, out bool s4);
                    success = s4;
                    return v4;
                case ConvertingFunctions.YFMarketCapToMillion:
                    string v5 = YFMarketCapToMillion(value, extraParam, out bool s5);
                    success = s5;
                    return v5;
                case ConvertingFunctions.EvalDate:
                    string v7 = EvalDate(value, out bool s7);
                    success = s7;
                    return v7;
                case ConvertingFunctions.Varchar50:
                    string v8 = Varchar50(value, extraParam, out bool s8);
                    success = s8;
                    return v8;
                case ConvertingFunctions.Varchar:
                    string v9 = Varchar(value, out bool s9);
                    success = s9;
                    return v9;

                default:
                    success = false;
                    return "NodeConverters.ConvertValue() failed";

            }
        }

        private static string Varchar(string dataPoint, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            success = true;

            return dataPoint;
        }

        private static string Varchar50(string dataPoint, string regex, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            success = true;

            string result;

            if (regex != null && dataPoint.Length > 0 && (new System.Text.RegularExpressions.Regex(regex).Matches(dataPoint).Count > 0))
                result = new System.Text.RegularExpressions.Regex(regex).Matches(dataPoint)[0].Value;
            else if (regex != null && dataPoint.Length > 0 && !(new System.Text.RegularExpressions.Regex(regex).Matches(dataPoint).Count > 0))
                return "NULL";
            else
                result = dataPoint; 

            return string.Concat("'", result.Substring(0, Math.Min(50, result.Length)), "'");
        }
                
        private static string EvalDate(string dataPoint, out bool success)
        {
            //przypadek N/A, para dat, inna para

            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            DateTime result;

            try
            {
                result = DateTime.Parse(dataPoint, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                //// //DateTime.Parse nie umie inaczej niz wyjatkie powiedziec, ze nie udalo sie parsowanie

                success = false;
                return "NULL";
            }

            success = true;
            return String.Concat("'", result.Date.ToString("yyyy-MM-dd"), "'");
        }

        private static string YFMarketCapToMillion(string dataPoint, string lowerLimit, out bool success) //specyficzne dla Yahoo Finance, dotyczy kapitalizacji, short scale
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = "";

            foreach (char c in dataPoint)
            {
                if (Char.IsDigit(c) || c == '.' || c == '-')
                    s += c;
            }

            success = Decimal.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            if (dataPoint.Contains('T'))
                result *= 1000000;
            else if (dataPoint.Contains('B'))
                result *= 1000;

            if (lowerLimit != null && Decimal.TryParse(lowerLimit, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal limit))
            {
                if (result <= limit)
                {
                    success = false;
                    return "NULL";
                }
            }

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        private static string EvalInt(string dataPoint, string lowerLimit, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = dataPoint.Trim();

            success = Int32.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out int result);

            if (lowerLimit != null && Decimal.TryParse(lowerLimit, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal limit))
            {
                if (result <= limit)
                {
                    success = false;
                    return "NULL";
                }
            }

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        private static string EvalDecimal(string dataPoint, string lowerLimit, out bool success)
        {
            if (dataPoint == null || dataPoint == "")
            {
                success = true;
                return "NULL";
            }

            string s = dataPoint.Trim();            

            success = Decimal.TryParse(s, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);

            //co jesli za duzo miejsc po przecinku i mozna by zaokrąglić?
            if (s.Length > EvalDecimalPrecisionLimit + 1 && result >= 0 || s.Length > EvalDecimalPrecisionLimit + 2 && result < 0)
            {
                success = false;
                return "NULL";
            }


            if (lowerLimit != null && Decimal.TryParse(lowerLimit, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal limit))
            {
                if (result <= limit)
                {
                    success = false;
                    return "NULL";
                }
            }

            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));

        }

        
        private static string DecimalRangeLeft(string dataPoint, string lowerLimit, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
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


            //tu jest sraka, ujawniła się po dodaniu remove empty entries
            //data point jest 'N/A (N/A)'
            //s jest '           '
            //kolekcja s.Split nie ma elementów //sprawdzanie: List.Any() jest true, jeżeli są jakieś elementy
            //tryparse dostaje jako parametr odwołanie do nieistniejącego elementu
            //dodatkowo przed zwróceniem result nie jest sprawdzany success
            //nie spałeś => nie koduj
            //poprawki nanieść w pozostałych podobnych funkcjach

            if (s.Split(' ').Any(a => !string.IsNullOrEmpty(a)))
            {
                success = Decimal.TryParse(s.Split(' ').First(a => !string.IsNullOrEmpty(a)), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);
                if (success)
                {
                    if (lowerLimit != null && Decimal.TryParse(lowerLimit, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal limit))
                    {
                        if (result <= limit)
                        {
                            success = false;
                            return "NULL";
                        }
                    }

                    return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
                }
            }
            
            success = false;
            return "NULL";
        }

        private static string DecimalRangeRight(string dataPoint, string lowerLimit, out bool success) //nie obsługuje ujemnych, aby obsługiwać "12.34 - 56.78"
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

            if (s.Split(' ').Any(a => !string.IsNullOrEmpty(a)))
            {
                success = Decimal.TryParse(s.Split(' ').Last(a => !string.IsNullOrEmpty(a)), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result);
                if (success)
                {
                    if (lowerLimit != null && Decimal.TryParse(lowerLimit, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal limit))
                    {
                        if (result <= limit)
                        {
                            success = false;
                            return "NULL";
                        }
                    }

                    return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
                }
            }
            
            success = false;
            return "NULL";
        }
    }
}
