using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using DotVVM.Samples.BasicSamples.Api.Common.Model;

namespace DotVVM.Samples.BasicSamples.Api.Common.DataStore
{
    public class DataGenerator
    {
        private Random r;

        public DataGenerator(int seed)
        {
            r = new Random(seed);
        }

        /// <summary>
        /// Gets a value between half of a specified maximum and the maximum.
        /// </summary>
        private int GetHalfToMaxRandom(int maximum)
        {
            return r.Next(maximum / 2, maximum + 1);
        }

        public string GetWords(int maxWords, int maxWordLength, Casing casing)
        {
            var sb = new StringBuilder();

            var wordsCount = GetHalfToMaxRandom(maxWordLength);
            for (int i = 0; i < wordsCount; i++)
            {
                var wordLength = GetHalfToMaxRandom(maxWordLength);

                if (i > 0)
                {
                    sb.Append(" ");
                }
                sb.Append(GetString(wordLength, casing));
            }

            return sb.ToString();
        }

        public string GetString(int maxLength, Casing casing)
        {
            var length = GetHalfToMaxRandom(maxLength);
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(GetChar(casing, i));
            }
            return sb.ToString();
        }

        private char GetChar(Casing casing, int index)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            if ((index == 0 && casing == Casing.FirstUpper) || (casing == Casing.AllUpper))
            {
                return chars[r.Next(chars.Length / 2)];
            }
            else if (casing == Casing.FirstUpper || casing == Casing.AllLower)
            {
                return chars[chars.Length / 2 + r.Next(chars.Length / 2)];
            }
            else
            {
                return chars[r.Next(chars.Length)];
            }
        }

        public T GetCollectionItem<T>(IList<T> collection)
        {
            return collection[r.Next(collection.Count)];
        }

        public DateTime GetDate(TimeSpan timeInPast, TimeSpan timeInFuture)
        {
            var min = (DateTime.Today - timeInPast).Date;
            var max = (DateTime.Today + timeInFuture).Date;
            var days = (max - min).Days;
            return min.AddDays(r.Next(days));
        }

        public DateTime GetDateTime(TimeSpan timeInPast, TimeSpan timeInFuture)
        {
            var min = (DateTime.Today - timeInPast).Date;
            var max = (DateTime.Today + timeInFuture).Date;
            var days = (max - min).TotalDays;
            return min.AddDays(r.NextDouble() * days);
        }

        public List<T> GetCollection<T>(int maxItems, Func<int, T> selector)
        {
            return Enumerable.Range(0, GetHalfToMaxRandom(maxItems))
                .Select(selector)
                .ToList();
        }

        public bool GetBoolean()
        {
            return r.NextDouble() >= 0.5;
        }

        public decimal GetDecimal(decimal min, decimal max)
        {
            return min + (max - min) * (decimal) r.NextDouble();
        }
    }

    public enum Casing
    {
        Random,
        AllLower,
        AllUpper,
        FirstUpper
    }
}
