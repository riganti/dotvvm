using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class RenderContext
    {
        public string SerializedViewModel { get; set; }

        public Hosting.RedwoodRequestContext RedwoodRequestContext { get; set; }

        public string CurrentPageArea { get; set; }

        public Stack<string> PathFragments { get; set; }



        public RenderContext()
        {
            CurrentPageArea = "root";
            PathFragments = new Stack<string>();
        }
    }
}