using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Tools.SeleniumGenerator;
using DotVVM.Framework.Tools.SeleniumGenerator.Helpers;

namespace DotVVM.Testing.SeleniumGenerator.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void SeleniumGenerator_TestRemovingNonIdentifierCharacters()
        {
            // initialize
            var testString = "__Ahoj1234@\'PTest Test";

            // do
            var resultString = SelectorStringHelper.RemoveNonIdentifierCharacters(testString);

            // assert
            var expectedString = "__Ahoj1234PTestTest";
            Assert.AreEqual(resultString, expectedString);
        }

        [TestMethod]
        public void SeleniumGenerator_TestAddDataContextPrefixesToName()
        {
            // initialize
            var dataContextPrefixes = new List<string> { "MyPage", "FirstTab", "Address" };
            var uniqueName = "City";

            // do
            var uniqueNameWithDataContextPrefixes = SelectorStringHelper.AddDataContextPrefixesToName(dataContextPrefixes, uniqueName);

            // assert
            var expectedResult = "MyPage_FirstTab_Address_City";
            Assert.AreEqual(expectedResult, uniqueNameWithDataContextPrefixes);
        }

        [TestMethod]
        public void SeleniumGenerator_TestNormalizeUniqueName()
        {
            // initialize
            var testString = "My superb String";

            // do
            var resultString = SelectorStringHelper.RemoveNonIdentifierCharacters(testString);

            // assert
            var expectedString = "MySuperbString";
            Assert.AreEqual(resultString, expectedString);
        }

        [TestMethod]
        public void SeleniumGenerator_CheckMakePropertyNameUnique()
        {
            // initialize
            var myGenerator = new MySeleniumGeneratorTestClass();
            var usedNames = new List<string> { "First", "Second", "Third" };
            var testString = "Second";
            var expectedString = "Second1";

            // do
            var resultString = myGenerator.MakePropertyNameUnique(usedNames, testString);

            // assert
            Assert.AreEqual(expectedString, resultString);
        }

        [TestMethod]
        public void SeleniumGenerator_MakePropertyNameUnique()
        {
            // initialize
            var myGenerator = new MySeleniumGeneratorTestClass();
            var usedNames = new List<string> { "First", "Second", "Third" };
            var testString = "Fourth";
            var expectedString = "Fourth";

            // do
            var resultString = myGenerator.MakePropertyNameUnique(usedNames, testString);

            // assert
            Assert.AreEqual(expectedString, resultString);
        }

        [TestMethod]
        public void SelectorStringHelper_SetFirstLetterUp()
        {
            // initialize
            var testString = "testString";
            var expectedString = "TestString";

            // do
            var resultString = SelectorStringHelper.SetFirstLetterUp(testString);

            // assert
            Assert.AreEqual(expectedString, resultString);
        }

        [TestMethod]
        public void SeleniumGeneratorOptions_AddCustomGenerator()
        {
            // initialize
            var options = new SeleniumGeneratorOptions();
            var myGenerator = new MySeleniumGeneratorTestClass();

            // do
            options.AddCustomGenerator(myGenerator);

            // assert
            var foundGenerator = options.GetCustomGenerators().FirstOrDefault(b => b.Equals(myGenerator));
            if (foundGenerator != null)
            {
                Assert.AreEqual(myGenerator, foundGenerator);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SeleniumGeneratorOptions_AddAssembly()
        {
            // initialize
            var options = new SeleniumGeneratorOptions();
            var myAssembly = new MySeleniumGeneratorTestClass().GetType().Assembly;

            // do
            options.AddAssembly(myAssembly);

            // assert
            var foundAssembly = options.GetAssemblies().FirstOrDefault(b => b.Equals(myAssembly));
            if (foundAssembly != null)
            {
                Assert.AreEqual(myAssembly, foundAssembly);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SeleniumGeneratorOptions_TryAddSameGenerators()
        {
            // initialize
            var options = new SeleniumGeneratorOptions();
            var myGenerator = new MySeleniumGeneratorTestClass();

            // do
            options.AddCustomGenerator(myGenerator);
            options.AddCustomGenerator(myGenerator);

            // assert
            if (options.GetCustomGenerators().Count == 1)
            {
                Assert.AreEqual(myGenerator, options.GetCustomGenerators().First());
            }
            else
            {
                Assert.Fail();
            }
        }
    }


    // new class because of SeleniumGenerator is abstract class
    public class MySeleniumGeneratorTestClass : SeleniumGenerator<HtmlGenericControl>
    {
        public override DotvvmProperty[] NameProperties => throw new System.NotImplementedException();

        public override bool CanUseControlContentForName => throw new System.NotImplementedException();

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
