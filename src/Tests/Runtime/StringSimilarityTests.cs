using System;
using System.Linq;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class StringSimilarityTests
    {
        [TestMethod]
        [DataRow("dotvvm", "dotvvm", 0)]
        [DataRow("kitten", "sitting", 3)]
        [DataRow("ab", "ba", 1)]
        [DataRow("Visible", "Visble", 1)]
        [DataRow("Visible", "Visbile", 1)]
        public void DamerauLevenshtein_IdenticalStrings_IsZero(string a, string b, int cost)
        {
            var d = StringSimilarity.DamerauLevenshteinDistance(a, b);
            Assert.AreEqual(cost, d);
            d = StringSimilarity.DamerauLevenshteinDistance(b, a);
            Assert.AreEqual(cost, d);
        }

        [TestMethod]
        public void SequenceAlignment_EqualSequences_AllMatched()
        {
            var a = "abc";
            var b = "abc";
            var aligned = StringSimilarity.SequenceAlignment<char>(a.AsSpan(), b.AsSpan(), (x, y) => x == y ? 0 : 1);

            XAssert.Equal([ ('a','a'), ('b','b'), ('c','c') ], aligned);
        }

        [TestMethod]
        public void SequenceAlignment_Gaps_Global()
        {
            int?[] a = [ 1, 2, 3 ];
            int?[] b = [ 1, 3 ];
            // global alignment -> gaps are expensive, but unavoidable
            var aligned = StringSimilarity.SequenceAlignment<int?>(a, b, (x, y) => x == y ? 0 : 1, gapCost: 50);

            XAssert.Equal([ (1, 1), (2, null), (3, 3) ], aligned);
        }

        [TestMethod]
        public void SequenceAlignment_PrefersGaps_WhenSubstitutionIsExpensive()
        {
            int?[] a = [ 1, 2, 3 ];
            int?[] b = [ 1, 4, 3 ];
            // mismatches very expensive -> alignment has only gaps
            var aligned = StringSimilarity.SequenceAlignment<int?>(a, b, (x, y) => x == y ? 0 : 100, gapCost: 1);

            // (null, 4), (2, null) order does not matter, but the algorithm currently always chooses this one
            XAssert.Equal([ (1, 1), (null, 4), (2, null), (3, 3) ], aligned);
        }

        [TestMethod]
        public void SequenceAlignment_HandlesEmptySequences()
        {
            int?[] a = [];
            int?[] b = [ 10, 20 ];
            var aligned = StringSimilarity.SequenceAlignment<int?>(a, b, (x, y) => x == y ? 0 : 1);

            XAssert.Equal([ (null, 10), (null, 20) ], aligned);
        }
    }
}
