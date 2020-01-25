#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    internal static class PropertyImmutableHashtable
    {
        static int HashCombine(int a, int b) => a + b;

        public static bool ContainsKey(DotvvmProperty?[] keys, int hashSeed, DotvvmProperty p)
        {
            var l = keys.Length;
            if (l == 4)
            {
                return keys[0] == p | keys[1] == p | keys[2] == p | keys[3] == p;
            }

            var lengthMap = l - 1; // trims the hash to be in bounds of the array
            var hash = HashCombine(p.GetHashCode(), hashSeed) & lengthMap;

            var i1 = hash & -2; // hash with last bit == 0 (-2 is something like ff...fe because two's complement)
            var i2 = hash | 1; // hash with last bit == 1

            return keys[i1] == p | keys[i2] == p;
        }

        public static int FindSlot(DotvvmProperty?[] keys, int hashSeed, DotvvmProperty p)
        {
            var l = keys.Length;
            if (l == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (keys[i] == p) return i;
                }
                return -1;
            }

            var lengthMap = l - 1; // trims the hash to be in bounds of the array
            var hash = HashCombine(p.GetHashCode(), hashSeed) & lengthMap;

            var i1 = hash & -2; // hash with last bit == 0 (-2 is something like ff...fe because two's complement)
            var i2 = hash | 1; // hash with last bit == 1

            if (keys[i1] == p) return i1;
            if (keys[i2] == p) return i2;
            return -1;
        }

        static ConcurrentDictionary<DotvvmProperty[], (int, DotvvmProperty?[])> tableCache = new ConcurrentDictionary<DotvvmProperty[], (int, DotvvmProperty?[])>(new EqCmp());

        class EqCmp : IEqualityComparer<DotvvmProperty[]>
        {
            public bool Equals(DotvvmProperty[] x, DotvvmProperty[] y)
            {
                if (x.Length != y.Length) return false;
                if (x == y) return true;
                for (int i = 0; i < x.Length; i++)
                    if (x[i] != y[i]) return false;
                return true;
            }

            public int GetHashCode(DotvvmProperty[] obj)
            {
                var h = obj.Length;
                foreach (var i in obj)
                    h = (i, h).GetHashCode();
                return h;
            }
        }

        // Some primes. Numbers not divisible by 2 should help shuffle the table in a different way every time.
        public static int[] hashSeeds = new [] {0, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541};

        public static (int hashSeed, DotvvmProperty?[] keys) BuildTable(DotvvmProperty[] a)
        {
            Debug.Assert(a.OrderBy(x => x.FullName).SequenceEqual(a));

            // make sure that all tables have the same keys so that they don't take much RAM (and remain in cache and make things go faster)
            return tableCache.GetOrAdd(a, keys => {
                if (keys.Length < 4)
                {
                    // just pad them to make things regular
                    var result = new DotvvmProperty[4];
                    Array.Copy(keys, result, keys.Length);
                    return (0, result);
                }
                else
                {
                    // first try closest size of power two
                    var size = 1 << (int)Math.Log(keys.Length + 1, 2);

                    while(true)
                    {
                        Debug.Assert((size & (size - 1)) == 0);
                        foreach (var hashSeed in hashSeeds)
                        {
                            var result = TryBuildTable(keys, size, hashSeed);
                            if (result != null)
                            {
                                Debug.Assert(TestTableCorrectness(keys, hashSeed, result));
                                return (hashSeed, result);
                            }
                        }

                        size *= 2;

                        if (size <= 4) throw new InvalidOperationException("Could not build hash table");
                    }

                }
            });
        }

        static bool TestTableCorrectness(DotvvmProperty[] keys, int hashSeed, DotvvmProperty?[] table)
        {
            return keys.All(k => FindSlot(table, hashSeed, k) >= 0);
        }

        /// <summary> Builds the core of the property hash table. Returns null if the table cannot be built due to collisions. </summary>
        static DotvvmProperty?[]? TryBuildTable(DotvvmProperty[] a, int size, int hashSeed)
        {
            var t = new DotvvmProperty?[size];
            var lengthMap = (size) - 1; // trims the hash to be in bounds of the array
            foreach (var k in a)
            {
                var hash = HashCombine(k.GetHashCode(), hashSeed) & lengthMap;

                var i1 = hash & -2; // hash with last bit == 0 (-2 is something like ff...fe because two's complement)
                var i2 = hash | 1; // hash with last bit == 1

                if (t[i1] == null)
                    t[i1] = k;
                else if (t[i2] == null)
                    t[i2] = k;
                else return null; // if neither of these slots work, we can't build the table
            }
            return t;
        }

        public static (int hashSeed, DotvvmProperty?[] keys, T[] valueTable) CreateTableWithValues<T>(DotvvmProperty[] properties, T[] values)
        {
            var (hashSeed, keys) = BuildTable(properties);
            var valueTable = new T[keys.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                valueTable[FindSlot(keys, hashSeed, properties[i])] = values[i];
            }
            return (hashSeed, keys, valueTable);
        }

        public static Action<DotvvmBindableObject> CreateBulkSetter(DotvvmProperty[] properties, object[] values)
        {
            var (hashSeed, keys, valueTable) = CreateTableWithValues(properties, values);
            return (obj) => obj.properties.AssignBulk(keys, valueTable, hashSeed);
        }
    }
}
