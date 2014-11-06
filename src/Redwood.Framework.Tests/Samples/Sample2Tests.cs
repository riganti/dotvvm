using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Tests.Samples.Sample2UIMapClasses;


namespace Redwood.Framework.Tests.Samples
{
    [CodedUITest]
    public class Sample2Tests
    {
        public Sample2Tests()
        {
        }

        [TestMethod]
        public void Sample2_BasicTest()
        {
            this.UIMap.LaunchSample();
            this.UIMap.CheckSCB();
            this.UIMap.VerifySCB();
            this.UIMap.CheckMCB1();
            this.UIMap.VerifyMCB1();
            this.UIMap.CheckMCB2();
            this.UIMap.VerifyMCB2();
            this.UIMap.CheckRB1();
            this.UIMap.VerifyRB1();
            this.UIMap.CheckRB2();
            this.UIMap.VerifyRB2();
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

        public Sample2UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new Sample2UIMap();
                }

                return this.map;
            }
        }

        private Sample2UIMap map;
    }
}
