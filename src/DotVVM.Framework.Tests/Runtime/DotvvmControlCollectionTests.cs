using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime.Misc;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmControlCollectionTests
    {

        [TestMethod]
        public void ControlCollection_ControlsCreatedOnInit()
        {
            var innerInitCalled = false;
            var root = new ControlLifeCycleMock
            {
                InitAction = (control, context) =>
                {
                    // generate a child control in the Init phase
                    control.Children.Add(new ControlLifeCycleMock()
                    {
                        InitAction = (control2, context2) =>
                        {
                            // test whether the Init on the inner control is called
                            innerInitCalled = true; 
                        }
                    });
                }
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, null);

            Assert.IsTrue(innerInitCalled);
        }


        [TestMethod]
        public void ControlCollection_ControlsCreatedOnLoad()
        {
            var innerInitCalled = false;
            var innerLoadCalled = false;
            var root = new ControlLifeCycleMock
            {
                LoadAction = (control, context) =>
                {
                    // generate a child control in the Load phase
                    control.Children.Add(new ControlLifeCycleMock()
                    {
                        InitAction = (control2, context2) =>
                        {
                            // test whether the Init on the inner control is called
                            innerInitCalled = true;
                        },
                        LoadAction = (control2, context2) =>
                        {
                            // test whether the Load on the inner control is called
                            innerLoadCalled = true;
                        }
                    });
                }
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, null);
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Load, null);

            Assert.IsTrue(innerInitCalled);
            Assert.IsTrue(innerLoadCalled);
        }


        [TestMethod]
        public void ControlCollection_InitializedAncestorModified()
        {
            var innerInitCalled = false;
            var root = new ControlLifeCycleMock
            {
                InitAction = (control, context) =>
                {
                    // generate a child control in the Init phase
                    control.Children.Add(new ControlLifeCycleMock());

                    // generate a second child that adds elements inside the first child
                    control.Children.Add(new ControlLifeCycleMock()
                    {
                        InitAction = (control2, context2) =>
                        {
                            control2.Parent.Children.First().Children.Add(new ControlLifeCycleMock()
                            {
                                InitAction = (control3, context3) =>
                                {
                                    // verify that the lately added control still calls the Init
                                    innerInitCalled = true;
                                }
                            });
                        }
                    });
                }
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, null);

            Assert.IsTrue(innerInitCalled);
        }


    }
}
