using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SampleReferenceAttribute : Attribute 
    {
        public string SampleName { get; }

        public SampleReferenceAttribute(string sampleName)
        {
            SampleName = sampleName;
        }

    }
}
