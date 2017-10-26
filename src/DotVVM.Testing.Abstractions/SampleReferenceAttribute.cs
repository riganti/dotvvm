using System;

namespace DotVVM.Testing.Abstractions
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
