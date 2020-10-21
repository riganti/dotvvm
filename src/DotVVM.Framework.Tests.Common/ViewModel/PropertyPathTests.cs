using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class PropertyPathTests
    {
        [TestMethod]
        public void PropertyPath_GetPropertyPath()
        {
            var viewModel = new SampleViewModel
            {
                Context = new TestDotvvmRequestContext
                {
                    Configuration = DotvvmTestHelper.CreateConfiguration(),
                    ModelState = new ModelState()
                }
            };

            viewModel.Context.ModelState.ValidationTarget = viewModel;

            var error = viewModel.AddModelError(vm => vm.Users[0].Post.PostText, "Testing validation error.");

            Assert.AreEqual("/Users[0]/Post/PostText", error.PropertyPath);
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
