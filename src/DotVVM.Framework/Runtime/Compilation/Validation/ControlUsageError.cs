using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Validation
{
    public class ControlUsageError
    {
        public string ErrorMessage { get; }
        public DothtmlNode[] Nodes { get; }
        public ControlUsageError(string message, IEnumerable<DothtmlNode> nodes)
        {
            ErrorMessage = message;
            Nodes = nodes.ToArray();
        }
        public ControlUsageError(string message, params DothtmlNode[] nodes) : this(message, (IEnumerable<DothtmlNode>)nodes) { }
    }
}
