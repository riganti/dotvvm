using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using DotVVM.Framework.ViewModel.Validation;
using System.Linq;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class PropertyPathTests
    {
        public Action MyProperty { get; set; } = () => { };

        SampleViewModel CreateViewModel(DotvvmRequestType requestType = DotvvmRequestType.Command)
        {
            var vm = new SampleViewModel
            {
                Context = DotvvmTestHelper.CreateContext(requestType: requestType)
            };
            vm.Context.ModelState.ValidationTarget = vm;
            return vm;
        }


        [TestMethod]
        public void PropertyPath_GetPropertyPath()
        {
            var viewModel = CreateViewModel();

            var error = ValidationErrorFactory.AddModelError(viewModel, vm => vm.Users[0].Post.PostText, "Testing validation error.");

            Assert.AreEqual("Users/0/Post/PostText", error.PropertyPath);
        }

        [TestMethod]
        public void PropertyPath_LocalVariable()
        {
            var viewModel = CreateViewModel();

            for (int i = 0; i < 10; i++)
            {
                if (i % 4 == 0)
                {
                    ValidationErrorFactory.AddModelError(viewModel, vm => vm.Users[i].Post.PostText, "Testing validation error.");
                }
            }

            var errors = viewModel.Context.ModelState.Errors;
            CollectionAssert.AreEqual(new [] { "Users/0/Post/PostText", "Users/4/Post/PostText", "Users/8/Post/PostText" }, errors.Select(e => e.PropertyPath).ToArray());
        }

        [TestMethod]
        public void PropertyPath_FailsOnInvalidExpression()
        {
            var viewModel = CreateViewModel();
            var ex = Assert.ThrowsException<ArgumentException>(() => ValidationErrorFactory.AddModelError(viewModel, vm => vm.Users[0].Post.PostText.Length + 14, "Testing validation error."));
            Assert.AreEqual("Provided path expression is invalid. Make sure it contains only property identifiers, member accesses and indexers with numeric literals.", ex.Message);

            ex = Assert.ThrowsException<ArgumentException>(() => ValidationErrorFactory.AddModelError(viewModel, vm => vm.Users.Select(e => e.Post), "Testing validation error."));
            Assert.AreEqual("Provided path expression is invalid. Make sure it contains only property identifiers, member accesses and indexers with numeric literals.", ex.Message);
        }

        public class SampleViewModel : DotvvmViewModelBase
        {
            public User[] Users { get; set; } =
            {
                new User
                {
                    Post = new Post
                    {
                        PostText = "Testing post"
                    }
                }
            };

            public class Post
            {
                public DateTime PostDate { get; set; }

                public string PostText { get; set; }

                public int UserId { get; set; }

                public override string ToString()
                {
                    return $"{nameof(UserId)}: {UserId}, {nameof(PostDate)}: {PostDate}, {nameof(PostText)}: {PostText}";
                }
            }

            public class User
            {
                public Post Post { get; set; }
            }
        }
    }
}
