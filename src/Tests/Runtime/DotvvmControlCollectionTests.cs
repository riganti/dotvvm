using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmControlCollectionTests
    {
        private IDotvvmRequestContext context = DotvvmTestHelper.CreateContext();

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
            root.SetValue(Internal.RequestContextProperty, context);

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, context);

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
            root.SetValue(Internal.RequestContextProperty, context);

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, context);
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Load, context);

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
                            control2.Parent.CastTo<DotvvmControl>().Children.First().Children.Add(new ControlLifeCycleMock()
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
            root.SetValue(Internal.RequestContextProperty, context);

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.Init, context);

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
            root.SetValue(Internal.RequestContextProperty, context);

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.PreRender, context);

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

        [TestMethod]
        public void ControlCollection_AddingToOptimizedControl()
        {
            var eventLog = new List<ControlLifeCycleEvent>();
            var root = new ControlLifeCycleMock(eventLog, "root")
            {
                InitAction = (control, context) =>
                {
                    control.Children.Add(new ControlLifeCycleMock(eventLog, "root_a")
                    {
                        LifecycleRequirements = ControlLifecycleRequirements.None,
                    });
                },
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.PreRender, context);

            var root_New = new ControlLifeCycleMock(eventLog, "root_New");
            root.Children.Add(root_New);

            var root_a_New = new ControlLifeCycleMock(eventLog, "root_a_New");
            root.Children.First().Children.Add(root_a_New);

            Assert.IsTrue(eventLog.Count(e => e.Control == root_a_New) == eventLog.Count(e => e.Control == root_New),
                "Control added to root has not the same number of event as control added to the optimized one.");
        }

        [TestMethod]
        public void ControlCollection_NestedControlsWithNoneRequirementParent()
        {
            var initCalled = false;
            var loadCalled = false;
            var preRenderCalled = false;

            var firstChild = new ControlLifeCycleMock() {
                LifecycleRequirements = ControlLifecycleRequirements.All,
            };

            var secondChild = new ControlLifeCycleMock() {
                LifecycleRequirements = ControlLifecycleRequirements.All,
                InitAction = (control, _) => {
                    initCalled = true;
                },
                LoadAction = (control, _) => {
                    loadCalled = true;
                },
                PreRenderAction = (control, _) => {
                    preRenderCalled = true;
                }
            };

            var root = new ControlLifeCycleMock();

            root.RenderAction = (control, _) => {
                var innerRoot = new ControlLifeCycleMock() { LifecycleRequirements = ControlLifecycleRequirements.None };
                root.Children.Add(innerRoot);

                var td1 = new ControlLifeCycleMock() { LifecycleRequirements = ControlLifecycleRequirements.None };
                innerRoot.Children.Add(td1);
                td1.Children.Add(firstChild);

                var td2 = new ControlLifeCycleMock() { LifecycleRequirements = ControlLifecycleRequirements.None };
                innerRoot.Children.Add(td2);
                td2.Children.Add(secondChild);
            };

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(root, LifeCycleEventType.PreRenderComplete, context);
            root.Render(null, null);

            Assert.IsTrue(initCalled);
            Assert.IsTrue(loadCalled);
            Assert.IsTrue(preRenderCalled);
        }
    }
}
