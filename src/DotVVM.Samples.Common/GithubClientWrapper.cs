using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Samples.BasicSamples
{
    public class GithubClientWrapper
    {
        public MyNamespace2.Client Root { get; set; }
        public MyNamespace2.CommitClient Commit { get; set; }
        public MyNamespace2.CodeClient Code { get; set; }
    }
}
