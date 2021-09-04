using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Validation
{
    public class ControlUsageError
    {
        public string ErrorMessage { get; }
        public DothtmlNode[] Nodes { get; }
        public ControlUsageError(string message, IEnumerable<DothtmlNode?> nodes)
        {
            ErrorMessage = message;
            Nodes = nodes.Where(n => n != null).ToArray()!;
        }
        public ControlUsageError(string message, params DothtmlNode?[] nodes) : this(message, nodes.AsEnumerable()) { }
    }
}
