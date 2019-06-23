using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public class TestEnvironmentHelper
    {

        public static string FindSolutionDirectory()
        {
            var path = Environment.CurrentDirectory;

            while (!File.Exists(Path.Combine(path, "DotVVM.Testing.SeleniumGenerator.sln")))
            {
                path = Path.GetDirectoryName(path);
                if (path.Length < 4)
                {
                    Assert.Fail("Solution directory could not be found!");
                }
            }

            return path;
        }

    }
}
