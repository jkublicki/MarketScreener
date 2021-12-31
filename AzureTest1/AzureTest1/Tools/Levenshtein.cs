using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketScreener.Tools
{
    public static class Levenshtein
    {
        public static int LevenshteinDistance(string s, string t)
        {
            int m = s.Length;
            int n = t.Length;

            int[,] d = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
                d[i, 0] = i;
            for (int j = 1; j <= n; j++)
                d[0, j] = j;

            for (int i = 1; i <= m; i++)
                for (int j = 1; j <= n; j++)
                {
                    int cost;
                    if (s[i - 1] == t[j - 1]) //tutaj string jest indeksowany od 0, inaczej niż w https://pl.wikipedia.org/wiki/Odleg%C5%82o%C5%9B%C4%87_Levenshteina
                        cost = 0;
                    else
                        cost = 1;

                    d[i, j] = new[] {
                        d[i - 1, j] + 1,       // usuwanie
                        d[i, j - 1] + 1,       // wstawianie
                        d[i - 1, j - 1] + cost}.Min();
                }

            return d[m, n];
        }
    }
}
