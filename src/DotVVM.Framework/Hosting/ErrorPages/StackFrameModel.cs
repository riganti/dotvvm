using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class StackFrameModel
    {
        public MethodBase Method { get; set; }
        public SourceModel At { get; set; }
    }
}
