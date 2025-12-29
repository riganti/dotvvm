using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class JsonUtilsTests
    {
        const string IrrelevantObjectStart = """
            "notthis": { "alsonotthis": "lala", "another": 1 },
            "alsonot": [ 1, 2, 3, { "irrelevant": 1, "nope": null, "neither_this": true, "alsonot": false }, [], [[[[], []]]] ],
            """;

        [DataTestMethod]
        [DataRow("{\"viewModel\":", "viewModel")]
        [DataRow(
            $$"""
            {
                "test": [
                    { "notthis": 1, "myprop": 5},
                    {
                        "notthis": 1,
                        "myprop":
            """,
            "test/1/myprop"
        )]
        [DataRow(
            $$"""
            { {{IrrelevantObjectStart}}
                "test": [
                    { "notthis": 1, "myprop": 5},
                    {
                        "notthis": 1,
                        "myprop":
            """,
            "test/1/myprop"
        )]
        [DataRow(
            $$"""
            { {{IrrelevantObjectStart}}
                "test": [ { {{IrrelevantObjectStart}} "myprop":{
            """,
            "test/0/myprop"
        )]
        [DataRow(
            $$"""
            { {{IrrelevantObjectStart}}
               "test2":
            """,
            "test2"
        )]
        [DataRow(
            $$"""
            { {{IrrelevantObjectStart}}
               "test3": { "ok": [1, 2, 3], "alsofine": {}
            """,
            "test3"
        )]
        [DataRow(
            $$"""
            {
                "items": [
                    { "id": 1 },
            """,
            "items/1"
        )]
        [DataRow(
            $$"""
            {
                "items": [
                    {
                        "nested": [
                            1,
                            2,
            """,
            "items/0/nested/2"
        )]
        [DataRow(
            $$"""
            {
                "items": [
            """,
            "items/0"
        )]
        [DataRow(
            $$"""
            { "arr": [[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[1, 2], []]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]],
                "prop": 1
            """,
            "prop"
        )]
        // Test cases for root-level arrays
        [DataRow(
            """
            [1, 2,
            """,
            "2"
        )]
        [DataRow(
            """
            [
            """,
            "0"
        )]
        [DataRow(
            """
            [{"a": 1}, {"b":
            """,
            "1/b"
        )]
        [DataRow(
            """
            [[1, 2], [3,
            """,
            "1/1"
        )]
        [DataRow(
            """
            [[[],
            """,
            "1"
        )]
        [DataRow(
            """
            [1, 2, 3
            """,
            ""
        )]
        public void GetInvalidJsonErrorPath(string json, string expectedPath)
        {
            var utf8 = StringUtils.Utf8.GetBytes(json);
            var path = SystemTextJsonUtils.GetFailurePath(utf8);
            Assert.AreEqual(expectedPath, string.Join("/", path));
        }
    }
}
