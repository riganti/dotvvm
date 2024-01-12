using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Validation
{
    /// <summary> Represents an error or a warning reported by a control usage validation method. </summary>
    /// <seealso cref="ControlUsageValidatorAttribute"/>
    public class ControlUsageError
    {
        public string ErrorMessage { get; }
        /// <summary> The error will be shown on the these syntax nodes. When empty, the start tag is underlined. </summary>
        public DothtmlNode[] Nodes { get; }
        /// <summary> Error - the page compilation will fail. Warning - the user will only be notified about the reported problem (in the log, for example). Other severities are currently not shown at all. </summary>
        public DiagnosticSeverity Severity { get; } = DiagnosticSeverity.Error;
        public ControlUsageError(string message, DiagnosticSeverity severity, IEnumerable<DothtmlNode?> nodes)
        {
            ErrorMessage = message;
            Nodes = nodes.Where(n => n != null).ToArray()!;
            Severity = severity;
        }
        public ControlUsageError(string message, IEnumerable<DothtmlNode?> nodes) : this(message, DiagnosticSeverity.Error, nodes) { }
        public ControlUsageError(string message, params DothtmlNode?[] nodes) : this(message, DiagnosticSeverity.Error, nodes.AsEnumerable()) { }
        public ControlUsageError(string message, DiagnosticSeverity severity, params DothtmlNode?[] nodes) : this(message, severity, nodes.AsEnumerable()) { }

        public override string ToString()
        {
            var core = $"{Severity} {ErrorMessage}";
            var someToken = Nodes.SelectMany(n => n.Tokens).FirstOrDefault();
            if (someToken == null)
                return core;
            else
                return $"{core} (at {someToken.LineNumber}:{someToken.ColumnNumber})";
        }
    }
}
