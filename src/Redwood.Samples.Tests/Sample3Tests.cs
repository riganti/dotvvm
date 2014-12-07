using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Samples.Tests.Sample3UIMapClasses;


namespace Redwood.Samples.Tests
{
    [CodedUITest]
    public class Sample3Tests
    {
        public Sample3Tests()
        {
        }

        [TestMethod]
        public void Sample3_BasicTest()
        {
            this.UIMap.LaunchSample();
            this.UIMap.ChangePrice();
            this.UIMap.CheckChangedPrice();
            this.UIMap.AddAndFillThreeLines();
            this.UIMap.Recalculate();
            this.UIMap.VerifyPriceExpectedValues.UIItem463PaneInnerText = "496";
            this.UIMap.VerifyPrice();
            this.UIMap.RemoveSecondLine();
            this.UIMap.VerifyPriceExpectedValues.UIItem463PaneInnerText = "463";
            this.UIMap.VerifyPrice();
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

        public Sample3UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new Sample3UIMap();
                }

                return this.map;
            }
        }

        private Sample3UIMap map;
    }
}
