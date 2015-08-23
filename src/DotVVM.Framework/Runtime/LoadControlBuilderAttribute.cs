using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime
{
    public class LoadControlBuilderAttribute : Attribute
    {
        public string FilePath { get; set; }
        public LoadControlBuilderAttribute(string filePath)
        {
            this.FilePath = filePath;
        }
    }
}
