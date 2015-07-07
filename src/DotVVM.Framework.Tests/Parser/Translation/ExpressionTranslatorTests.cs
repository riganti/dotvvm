using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Parser.Translation;

namespace DotVVM.Framework.Tests.Parser.Translation
{
    [TestClass]
    public class ExpressionTranslatorTests
    {

        [TestMethod]
        public void ExpressionTranslator_Valid_BinaryOperators()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("FirstName() + LastName()", translator.Translate(" FirstName +   LastName "));
            Assert.AreEqual("FirstName() - LastName()", translator.Translate("FirstName-LastName"));
            Assert.AreEqual("FirstName() * LastName()", translator.Translate("FirstName * LastName"));
            Assert.AreEqual("FirstName() / LastName()", translator.Translate("FirstName / LastName"));
            Assert.AreEqual("FirstName() % LastName()", translator.Translate("FirstName % LastName"));
            Assert.AreEqual("FirstName() < LastName()", translator.Translate("FirstName < LastName"));
            Assert.AreEqual("FirstName() <= LastName()", translator.Translate("FirstName <= LastName"));
            Assert.AreEqual("FirstName() > LastName()", translator.Translate("FirstName > LastName"));
            Assert.AreEqual("FirstName() >= LastName()", translator.Translate("FirstName >= LastName"));
            Assert.AreEqual("FirstName() === LastName()", translator.Translate("FirstName == LastName"));
            Assert.AreEqual("FirstName() !== LastName()", translator.Translate("FirstName != LastName"));
        }

        [TestMethod]
        public void ExpressionTranslator_Valid_UnaryOperators()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("-FirstName()", translator.Translate("-FirstName"));
            Assert.AreEqual("!FirstName()", translator.Translate("!FirstName"));
        }

        [TestMethod]
        public void ExpressionTranslator_Valid_ExpressionWithParenthesis()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("Title() * (FirstName() / LastName())", translator.Translate("Title * (FirstName / LastName)"));
        }

        [TestMethod]
        public void ExpressionTranslator_Valid_ExpressionWithDot()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("(Customer()||{}).Name", translator.Translate("Customer.Name"));
        }

        [TestMethod]
        public void ExpressionTranslator_Valid_ExpressionWithDots()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("((Customer()||{}).Name()||{}).FirstName", translator.Translate("Customer.Name.FirstName"));
        }
        
        [TestMethod]
        public void ExpressionTranslator_Valid_StringLiteral()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("IsCompleted() ? \"completed\" : \"\"", translator.Translate("IsCompleted ? \"completed\" : \"\""));
        }

        [TestMethod]
        public void ExpressionTranslator_Valid_NumericLiteral()
        {
            var translator = new ExpressionTranslator();
            Assert.AreEqual("IsCompleted() ? 1 : 1.64", translator.Translate("IsCompleted ? 1 : 1.64"));
        }
    }
}
