using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Tests
{
    [TestClass]
    public class ResourceManagementTests
    {
        [TestMethod]
        public void ResourceManager_List()
        {
            var rm = new ResourceManager();
            var s = new ScriptResource("/Script/something.js");
            rm.AddResource(s);
            Assert.AreEqual(rm.Resources[0], s);
        }

        [TestMethod]
        public void ResourceManager_Prerequisities()
        {
            var rm = new ResourceManager();
            var s = new ScriptResource("/Script/something.js", new[] {
                new ScriptResource("/Script/a.js"),
                new ScriptResource("/Script/b.js"),
            });
            rm.AddResource(s);
            Assert.AreEqual(rm.Resources.Count, 3);
        }

        [TestMethod]
        public void ResourceManager_Set()
        {
            var rm = new ResourceManager();
            var s = new ScriptResource("/Script/something.js", new[] {
                new ScriptResource("/Script/a.js"),
                new ScriptResource("/Script/b.js"),
            });
            rm.AddResource(new ScriptResource("/Script/a.js"));
            rm.AddResource(s);
            rm.AddResource(new ScriptResource("/Script/b.js"));
            Assert.AreEqual(rm.Resources.Count, 3);
        }

        [TestMethod]
        public void ResourceManager_Render()
        {
            var rm = new ResourceManager();
            var s = new ScriptResource("/Script/something.js", new[] {
                new ScriptResource("/Script/a.js"),
                new ScriptResource("/Script/b.js"),
            });
            rm.AddResource(s);
            using (var t = new StringWriter())
            {
                var w = new HtmlWriter(t);
                rm.RenderLinks(w);
                var res = t.ToString();
                Assert.AreEqual(res.Replace(" ", "").Replace("\n", "").Replace("\r", ""),
                    "<script src=\"/Script/a.js\" type=\"text/javascript\"></script><script src=\"/Script/b.js\" type=\"text/javascript\"></script><script src=\"/Script/something.js\" type=\"text/javascript\"></script>".Replace(" ", ""));
            }
        }
    }
}
