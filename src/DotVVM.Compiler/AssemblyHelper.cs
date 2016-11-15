using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Compiler
{
    public static class AssemblyHelper
    {
        public static Assembly LoadReadOnly(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return Assembly.Load(bytes);
        }
    }
}
