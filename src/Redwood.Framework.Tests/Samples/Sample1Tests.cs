using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Tests.Samples.Sample1UIMapClasses;


namespace Redwood.Framework.Tests.Samples
{
    [CodedUITest]
    public class Sample1Tests
    {
        public Sample1Tests()
        {
        }

        [TestMethod]
        public void Sample1_BasicTest()
        {
            this.UIMap.LaunchSample();
            this.UIMap.AddTask();
            this.UIMap.FinishLastTask();
            this.UIMap.VerifyLastTaskFinished();
            this.UIMap.FinishFirstTask();
            this.UIMap.VerifyFirstTaskFinished();
            this.UIMap.CloseBrowser();
        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public Sample1UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new Sample1UIMap();
                }

                return this.map;
            }
        }

        private Sample1UIMap map;
    }
}
