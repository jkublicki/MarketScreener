using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;

namespace MarketScreener
{
    public static class GICS
    {
        private struct GICSItem
        {
            public string Category;
            public string Name;
            public string OtherName1;
            public int ID;
            public int Parent;
        }

        private static List<GICSItem> enuGICS; 

        public static (int, string) GetGICSCodeAndCategory(string descriptionGICS)
        {
            Console.WriteLine(descriptionGICS);


            const int maxLD = 2;
            string desc = descriptionGICS.ToUpper();

            if (enuGICS == null)
            {
                FillEnuGICS();
            }

            if (enuGICS.Any(item => item.Name == desc))
            {
                int id = enuGICS.FirstOrDefault(item => item.Name == desc).ID;
                string category = enuGICS.FirstOrDefault(item => item.ID == id).Category;
                return (id, category);
            }
            //jeżeli Name 'prawie' pasuje, gdzie prawie jest określone dystansem Levenshteina 
            else if (enuGICS.Any(item => Tools.Levenshtein.LevenshteinDistance(item.Name, desc) <= maxLD))
            {
                int id = enuGICS.FirstOrDefault(item => Tools.Levenshtein.LevenshteinDistance(item.Name, desc) <= maxLD).ID;
                string category = enuGICS.FirstOrDefault(item => item.ID == id).Category;
                return (id, category);
            }
            else if (enuGICS.Any(item => item.OtherName1 == desc))
            {
                int id = enuGICS.FirstOrDefault(item => item.OtherName1 == desc).ID;
                string category = enuGICS.FirstOrDefault(item => item.ID == id).Category;
                return (id, category);
            }
            else
                return (-1, "");
        }

        private static void FillEnuGICS()
        {
            enuGICS = new List<GICSItem>();
            string query = "SELECT ID, Name, OtherName1 FROM ENU_GICS";
            if (QueryDatabase.ExecuteSQLStatement(Secrets.ConnectionString, query, false, out DataTable dt) == -1)
            {
                Debug.WriteLine("Failed to execute SQL statement: " + query);
            }

            foreach (DataRow row in dt.Rows)
            {
                if (row != null && !row.IsNull(0))
                {
                    GICSItem item = new GICSItem();
                    item.ID = Convert.ToInt32(row["ID"]);
                    if (!row.IsNull("Name"))
                        item.Name = row["Name"].ToString();
                    else
                        item.Name = "";
                    if (!row.IsNull("OtherName1"))
                        item.OtherName1 = row["OtherName1"].ToString();
                    else
                        item.OtherName1 = "";
                    int len = item.ID.ToString().Length;
                    switch (len)
                    {
                        case 2: //Sektory mają dwuznakowe kody
                            item.Category = "Sector";
                            break;
                        case 4:
                            item.Category = "Industry Group";
                            break;
                        case 6:
                            item.Category = "Industry";
                            break;
                        case 8:
                            item.Category = "Sub-Industry";
                            break;
                        default:
                            item.Category = "";
                            break;
                    }
                    enuGICS.Add(item);
                }
            }
        }


   



    }
}
