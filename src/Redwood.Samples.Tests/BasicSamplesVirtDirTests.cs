using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Redwood.Samples.Tests
{
    [TestClass]
    public class BasicSamplesVirtDirTests : BasicSamplesTests
    {
        protected override string BaseUrl
        {
            get { return "http://localhost:8627/redwoodSamples/"; }
        }


        [TestMethod]
        public void Sample1Test_VirtDir() { Sample1Test(); }
        [TestMethod]
        public void Sample2Test_VirtDir() { Sample2Test(); }
        [TestMethod]
        public void Sample3Test_VirtDir() { Sample3Test(); }
        [TestMethod]
        public void Sample4Test_VirtDir() { Sample4Test(); }
        [TestMethod]
        public void Sample5Test_VirtDir() { Sample5Test(); }
        [TestMethod]
        public void Sample6Test_VirtDir() { Sample6Test(); }
        [TestMethod]
        public void Sample7Test_VirtDir() { Sample7Test(); }
        [TestMethod]
        public void Sample8Test_VirtDir() { Sample8Test(); }
        [TestMethod]
        public void Sample9Test_VirtDir() { Sample9Test(); }
        [TestMethod]
        public void Sample10Test_VirtDir() { Sample10Test(); }
        [TestMethod]
        public void Sample11Test_VirtDir() { Sample11Test(); }
        [TestMethod]
        public void Sample12Test_VirtDir() { Sample12Test(); }
        [TestMethod]
        public void Sample13Test_VirtDir() { Sample13Test(); }
        [TestMethod]
        public void Sample14Test_VirtDir() { Sample14Test(); }
        [TestMethod]
        public void Sample15Test_VirtDir() { Sample15Test(); }
        [TestMethod]
        public void Sample16Test_VirtDir() { Sample16Test(); }
        [TestMethod]
        public void Sample17Test_VirtDir() { Sample17Test(); }
        [TestMethod]
        public void Sample18Test_VirtDir() { Sample18Test(); }

    }
}