using System;

namespace DotVVM.Framework.Compilation
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
