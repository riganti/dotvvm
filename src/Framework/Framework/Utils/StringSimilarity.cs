using System;

namespace DotVVM.Framework.Utils
{
    internal static class StringSimilarity
    {
        /// <summary> Edit distance with deletion (Visble), insertion (Visivble), substitution (Visine) and transposition (Visilbe) </summary>
        public static int DamerauLevenshteinDistance(string a, string b)
        {
            // https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance

            if (a.Length * b.Length > 100_000)
                // avoid comparing long strings
                return a.Length + b.Length;

            var d = new int[a.Length + 1, b.Length + 1];

            for (var i = 0; i <= a.Length; i++)
                d[i, 0] = i;
                
            for (var j = 0; j <= b.Length; j++)
                d[0, j] = j;
            
            for (var i = 0; i < a.Length; i ++)
            {
                for (var j = 0; j < b.Length; j++)
                {
                    var substitutionCost = a[i] == b[j] ? 0 : 1;
                    var deletionCost = 1;
                    var insertionCost = 1;
                    d[i+1, j+1] = min(d[i, j+1] + deletionCost,
                                  d[i+1, j] + insertionCost,
                                  d[i, j] + substitutionCost);
                    if (i > 1 && j > 1 && a[i] == b[j-1] && a[i-1] == b[j])
                    {
                        var transpositionCost = 1;
                        d[i+1, j+1] = min(d[i+1, j+1],
                                      d[i-1, j-1] + transpositionCost);
                    }
                }
            }
            return d[a.Length, b.Length];
        }

        static int min(int a, int b) => Math.Min(a, b);
        static int min(int a, int b, int c) => min(min(a, b), c);
    }
}
