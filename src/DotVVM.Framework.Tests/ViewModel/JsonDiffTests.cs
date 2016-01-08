using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class JsonDiffTests
    {

        [TestMethod]
        public void JsonDiff_SimpleTest()
        {
            var a = JObject.Parse("{name:'djsfsh',ahoj:45}");
            var b = JObject.Parse("{name:'djsfsh',ahoj:42}");
            var diff = JsonUtils.Diff(a, b);
            JsonUtils.Patch(a, diff);
            Assert.IsTrue(JToken.DeepEquals(a, b));
        }
    }
}
