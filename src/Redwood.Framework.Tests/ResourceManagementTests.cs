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
        public void ResourceRepository_CommonResources()
        {
            var repo = new RwResourceRepository();
            repo.RegisterCommonResources();
        }

        [TestMethod]
        public void ResourceRepository_Resolve()
        {
            var repo = new RwResourceRepository();
            repo.RegisterCommonResources();
            var script = new ScriptResource("Script/test.js", "jquery");
            repo.Register("test", script);
            Assert.AreEqual(script, repo.Resolve("test"));
        }

        [TestMethod]
        public void ResourceRepository_Nest()
        {
            var repo = new RwResourceRepository();
            repo.RegisterCommonResources();
            var script1 = new ScriptResource("Script/test.js", "jquery");
            var script2 = new ScriptResource("Script/test42.js", "jquery");
            var nest = repo.Nest();
            repo.Register("test", script1);
            nest.Register("test", script2);
            Assert.AreEqual(script1, repo.Resolve("test"));
            Assert.AreEqual(script2, nest.Resolve("test"));
        }

        [TestMethod]
        public void ResourceManager_List()
        {
            var rr = new RwResourceRepository();
            rr.RegisterCommonResources();
            var rm = new RwResourceManager(rr);
            rm.AddResource("jquery");
            Assert.AreEqual(rm.Resources[0], "jquery");
        }

        [TestMethod]
        public void ResourceManager_Prerequisities()
        {
            var rr = new RwResourceRepository();
            rr.RegisterCommonResources();
            var rm = new RwResourceManager(rr);

            rm.AddResource("knockout");
            Assert.IsTrue(rm.Resources.Contains("jquery"));
        }

        [TestMethod]
        public void ResourceManager_Set()
        {
            var rr = new RwResourceRepository();
            var rm = new RwResourceManager(rr);
            rr.Register("little1", new ScriptResource("/Script/a.js"));
            rr.Register("little2", new ScriptResource("/Script/b.js"));
            rr.Register("big", new ScriptResource("/Script/something.js", 
                new string[] { "little1", "little2" }));

            rm.AddResource("little1");
            rm.AddResource("big");
            rm.AddResource("little1");

            Assert.AreEqual(rm.Resources.Count, 3);
        }

        [TestMethod]
        public void ResourceManager_Render()
        {
            var rr = new RwResourceRepository();
            var rm = new RwResourceManager(rr);
            rr.Register("little1", new ScriptResource("/Script/a.js"));
            rr.Register("little2", new ScriptResource("/Script/b.js"));
            rr.Register("big", new ScriptResource("/Script/something.js",
                new string[] { "little1", "little2" }));

            rm.AddResource("big");
            using (var t = new StringWriter())
            {
                var w = new HtmlWriter(t);
                rm.Render(w);
                var res = t.ToString();
                Assert.AreEqual(res.Replace(" ", "").Replace("\n", "").Replace("\r", ""),
                    "<script src=\"/Script/a.js\" type=\"text/javascript\"></script><script src=\"/Script/b.js\" type=\"text/javascript\"></script><script src=\"/Script/something.js\" type=\"text/javascript\"></script>".Replace(" ", ""));
            }
        }
    }
}
