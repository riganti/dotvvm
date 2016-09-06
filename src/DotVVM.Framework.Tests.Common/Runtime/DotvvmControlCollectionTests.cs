using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init);

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

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init);
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Load);

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

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init);

            Assert.IsTrue(innerInitCalled);
        }


        [TestMethod]
        public void ControlCollection_ControlsDeepNesting()
        {
            var eventLog = new List<ControlLifeCycleEvent>();
            var root = new ControlLifeCycleMock(eventLog, "root")
            {
                LoadAction = (control, context) =>
                {
                    control.Children.Add(new ControlLifeCycleMock(eventLog, "root_a")
                    {
                        LoadAction = (control2, context2) =>
                        {
                            control2.Children.Add(new ControlLifeCycleMock(eventLog, "root_a_b")
                            {
                                LoadAction = (control3, context3) =>
                                {
                                    control3.Children.Add(new ControlLifeCycleMock(eventLog, "root_a_b_c")
                                    {
                                        InitAction = (control4, context4) =>
                                        {
                                            control4.Children.Add(new ControlLifeCycleMock(eventLog, "root_a_b_c_d"));
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.PreRender);

            var index = 0;
            Action<string, LifeCycleEventType, bool> verifyAction = (name, eventType, isEntering) =>
            {
                Assert.AreEqual(name, eventLog[index].Control.Name);
                Assert.AreEqual(eventType, eventLog[index].EventType);
                Assert.AreEqual(isEntering, eventLog[index].IsEntering);
                index++;
            };

            verifyAction("root", LifeCycleEventType.PreInit, true);
            verifyAction("root", LifeCycleEventType.PreInit, false);
            verifyAction("root", LifeCycleEventType.Init, true);
            verifyAction("root", LifeCycleEventType.Init, false);
            verifyAction("root", LifeCycleEventType.Load, true);
            verifyAction("root_a", LifeCycleEventType.PreInit, true);
            verifyAction("root_a", LifeCycleEventType.PreInit, false);
            verifyAction("root_a", LifeCycleEventType.Init, true);
            verifyAction("root_a", LifeCycleEventType.Init, false);
            verifyAction("root", LifeCycleEventType.Load, false);
            verifyAction("root_a", LifeCycleEventType.Load, true);
            verifyAction("root_a_b", LifeCycleEventType.PreInit, true);
            verifyAction("root_a_b", LifeCycleEventType.PreInit, false);
            verifyAction("root_a_b", LifeCycleEventType.Init, true);
            verifyAction("root_a_b", LifeCycleEventType.Init, false);
            verifyAction("root_a", LifeCycleEventType.Load, false);
            verifyAction("root_a_b", LifeCycleEventType.Load, true);
            verifyAction("root_a_b_c", LifeCycleEventType.PreInit, true);
            verifyAction("root_a_b_c", LifeCycleEventType.PreInit, false);
            verifyAction("root_a_b_c", LifeCycleEventType.Init, true);
            verifyAction("root_a_b_c_d", LifeCycleEventType.PreInit, true);
            verifyAction("root_a_b_c_d", LifeCycleEventType.PreInit, false);
            verifyAction("root_a_b_c", LifeCycleEventType.Init, false);
            verifyAction("root_a_b_c_d", LifeCycleEventType.Init, true);
            verifyAction("root_a_b_c_d", LifeCycleEventType.Init, false);
            verifyAction("root_a_b", LifeCycleEventType.Load, false);
            verifyAction("root_a_b_c", LifeCycleEventType.Load, true);
            verifyAction("root_a_b_c", LifeCycleEventType.Load, false);
            verifyAction("root_a_b_c_d", LifeCycleEventType.Load, true);
            verifyAction("root_a_b_c_d", LifeCycleEventType.Load, false);
            verifyAction("root", LifeCycleEventType.PreRender, true);
            verifyAction("root", LifeCycleEventType.PreRender, false);
            verifyAction("root_a", LifeCycleEventType.PreRender, true);
            verifyAction("root_a", LifeCycleEventType.PreRender, false);
            verifyAction("root_a_b", LifeCycleEventType.PreRender, true);
            verifyAction("root_a_b", LifeCycleEventType.PreRender, false);
            verifyAction("root_a_b_c", LifeCycleEventType.PreRender, true);
            verifyAction("root_a_b_c", LifeCycleEventType.PreRender, false);
            verifyAction("root_a_b_c_d", LifeCycleEventType.PreRender, true);
            verifyAction("root_a_b_c_d", LifeCycleEventType.PreRender, false);
        }
    }
}
