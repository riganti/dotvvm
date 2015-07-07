using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelValidatorTests
    {

        [TestMethod]
        public void ViewModelValidator_SimpleObject()
        {
            var testViewModel = new TestViewModel();
            var validator = new ViewModelValidator();
            var results = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Text", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ObjectWithCollection()
        {
            var testViewModel = new TestViewModel()
            {
                Children = new List<TestViewModel2>()
                {
                    new TestViewModel2() { Code = "012" },
                    new TestViewModel2() { Code = "ABC", Id = 13 },
                    new TestViewModel2() { Code = "345", Id = 15 }
                },
                Child = new TestViewModel2() {  Code = "123" }
            };
            var validator = new ViewModelValidator();
            var results = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("Child().Id", results[0].PropertyPath);
            Assert.AreEqual("Children()[0].Id", results[1].PropertyPath);
            Assert.AreEqual("Children()[1].Code", results[2].PropertyPath);
            Assert.AreEqual("Text", results[3].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ServerOnlyRules()
        {
            var testViewModel = new TestViewModel3() { Email = "aaa" };
            var validator = new ViewModelValidator();
            var results = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Email", results[0].PropertyPath);
        }


        public class TestViewModel
        {

            [Required]
            public string Text { get; set; }

            public List<TestViewModel2> Children { get; set; }

            public TestViewModel2 Child { get; set; }
        }

        public class TestViewModel2
        {
            [Required]
            public int? Id { get; set; }

            [RegularExpression("^[0-9]{3}$")]
            public string Code { get; set; }
        }

        public class TestViewModel3
        {
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}
