using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ChildViewModelInvokeMethodsTests
    {
        IDotvvmViewModel sutViewModel;

        [TestInitialize()]
        public void TestStartup()
        {
            sutViewModel = new TestViewModel();
        }

        [TestMethod]
        public void WhenViewModelInitIsRaisedThenShouldBeRaisedChildViewModelsInit()
        {
            sutViewModel.Init();
            Assert.AreEqual(1, ((TestViewModel)sutViewModel).UserControlViewModel.InitWasCalled);
        }

        [TestMethod]
        public void WhenViewModelInitIsRaisedThenShouldBeRaisedChildViewModelsInCollectionInit()
        {
            sutViewModel.Init();
            Assert.IsTrue(((TestViewModel)sutViewModel).UserControlViewModels.All(vm => vm.InitWasCalled == 1));
        }

        [TestMethod]
        public void WhenViewModelLoadIsRaisedThenShouldBeRaisedChildViewModelsLoad()
        {
            sutViewModel.Load();
            Assert.AreEqual(1, ((TestViewModel)sutViewModel).UserControlViewModel.LoadWasCalled);
        }

        [TestMethod]
        public void WhenViewModelLoadIsRaisedThenShouldBeRaisedChildViewModelsInCollectionLoad()
        {
            sutViewModel.Load();
            Assert.IsTrue(((TestViewModel)sutViewModel).UserControlViewModels.All(vm => vm.LoadWasCalled == 1));
        }

        [TestMethod]
        public void WhenViewModelPreRenderIsRaisedThenShouldBeRaisedChildViewModelsPreRender()
        {
            sutViewModel.PreRender();
            Assert.AreEqual(1, ((TestViewModel)sutViewModel).UserControlViewModel.PreRenderWasCalled);
        }

        [TestMethod]
        public void WhenViewModelPreRenderIsRaisedThenShouldBeRaisedChildViewModelsInCollectionPreRender()
        {
            sutViewModel.PreRender();
            Assert.IsTrue(((TestViewModel)sutViewModel).UserControlViewModels.All(vm => vm.PreRenderWasCalled == 1));
        }

        class TestUserControlViewModel : DotvvmViewModelBase
        {
            public int InitWasCalled { get; private set; } = 0;

            public int LoadWasCalled { get; private set; } = 0;

            public int PreRenderWasCalled { get; private set; } = 0;

            public override Task Init()
            {
                InitWasCalled++;
                return base.Init();
            }

            public override Task Load()
            {
                LoadWasCalled++;
                return base.Init();
            }
            public override Task PreRender()
            {
                PreRenderWasCalled++;
                return base.Init();
            }
        }

        class TestViewModel : DotvvmViewModelBase
        {
            public TestUserControlViewModel UserControlViewModel { get; } = new TestUserControlViewModel();
            public List<TestUserControlViewModel> UserControlViewModels { get; } = new List<TestUserControlViewModel> { new TestUserControlViewModel(), new TestUserControlViewModel(), new TestUserControlViewModel() };
        }
    }
}
