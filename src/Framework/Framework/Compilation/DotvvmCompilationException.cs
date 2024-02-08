using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Represents a failed dotvvm compilation result. The exception contains a list of all errors and warnings (<see cref="AllDiagnostics"/>). For the exception message, one error is selected as the "primary", usually it's the first encountered error. </summary>
    [Serializable]
    public class DotvvmCompilationException : Exception, IDotvvmException
    {
        public string? FileName
        {
            get => CompilationError.Location.FileName;
            set => SetFile(value, null);
        }
        [JsonIgnore]
        public MarkupFile? MarkupFile => CompilationError.Location.MarkupFile;
        [JsonIgnore]
        public string? SystemFileName => FileName?.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        /// <summary> Affected tokens of the primary compilation error. </summary>
        [JsonIgnore]
        public ImmutableArray<TokenBase> Tokens => CompilationError.Location?.Tokens ?? ImmutableArray<TokenBase>.Empty;
        /// <summary> Line number of the primary compilation error. </summary>
        public int? LineNumber => CompilationError.Location?.LineNumber;
        /// <summary> Position on the line of the primary compilation error. </summary>
        [JsonIgnore]
        public int? ColumnNumber => CompilationError.Location?.ColumnNumber;

        /// <summary> Text of the affected tokens of the first error. </summary>
        [JsonIgnore]
        public string[] AffectedSpans => CompilationError.Location?.AffectedSpans ?? Array.Empty<string>();

        /// <summary> The primary compilation error. </summary>
        public DotvvmCompilationDiagnostic CompilationError { get; set; }
        /// <summary> All diagnostics except the primary compilation error. </summary>
        public List<DotvvmCompilationDiagnostic> OtherDiagnostics { get; } = new List<DotvvmCompilationDiagnostic>();

        [JsonIgnore]
        public IEnumerable<DotvvmCompilationDiagnostic> AllDiagnostics => Enumerable.Concat(new [] { CompilationError }, OtherDiagnostics);
        [JsonIgnore]
        public IEnumerable<DotvvmCompilationDiagnostic> AllErrors => Enumerable.Concat(new [] { CompilationError }, OtherDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        Exception IDotvvmException.TheException => this;

        DotvvmProperty? IDotvvmException.RelatedProperty => this.CompilationError.Location.RelatedProperty;

        DotvvmBindableObject? IDotvvmException.RelatedControl => null;

        IBinding? IDotvvmException.RelatedBinding => this.CompilationError.Location.RelatedBinding;

        ResolvedTreeNode? IDotvvmException.RelatedResolvedControl => this.CompilationError.Location.RelatedResolvedNode;

        DothtmlNode? IDotvvmException.RelatedDothtmlNode => this.CompilationError.Location.RelatedSyntaxNode;

        IResource? IDotvvmException.RelatedResource => null;

        DotvvmLocationInfo? IDotvvmException.Location => this.CompilationError.Location.ToRuntimeLocation();

        public DotvvmCompilationException(string message) : this(message, innerException: null) { }

        public DotvvmCompilationException(string message, Exception? innerException) : base(message, innerException)
        {
            CompilationError = new DotvvmCompilationDiagnostic(message, DiagnosticSeverity.Error, null, innerException: innerException);
        }

        public DotvvmCompilationException(string message, Exception? innerException, IEnumerable<TokenBase>? tokens) : base(message, innerException)
        {
            var location = tokens is null ? DotvvmCompilationSourceLocation.Unknown : new DotvvmCompilationSourceLocation(tokens);
            CompilationError = new DotvvmCompilationDiagnostic(message, DiagnosticSeverity.Error, location, innerException: innerException);
        }

        public DotvvmCompilationException(DotvvmCompilationDiagnostic primaryError, IEnumerable<DotvvmCompilationDiagnostic> allDiagnostics) : base(primaryError.Message, primaryError.InnerException)
        {
            this.CompilationError = primaryError;
            this.OtherDiagnostics = allDiagnostics.Where(d => (object)primaryError != d).ToList(); 
        }

        public DotvvmCompilationException(string message, IEnumerable<TokenBase>? tokens) : this(message, null, tokens) { }
        protected DotvvmCompilationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            CompilationError = new DotvvmCompilationDiagnostic(this.Message, DiagnosticSeverity.Error, null, innerException: this.InnerException);
        }

        /// <summary> Creates a compilation error if the provided list of diagnostics contains an error. </summary>
        public static DotvvmCompilationException? TryCreateFromDiagnostics(IEnumerable<DotvvmCompilationDiagnostic> diagnostics)
        {
            // we sort by the end position of the error range to prefer more specific errors in case there is an overlap
            // for example, binding have 2 errors, one for the entire binding and a more specific error highlighting the problematic binding token
            var sorted = diagnostics.OrderBy(e => (-e.Priority, e.Location.EndLineNumber ?? int.MaxValue, e.Location.EndColumnNumber ?? int.MaxValue)).ToArray();
            if (sorted.FirstOrDefault(e => e.IsError) is {} error)
            {
                return new DotvvmCompilationException(error, sorted);
            }
            return null;
        }

        public void SetFile(string? fileName, MarkupFile? file)
        {
            if (fileName == CompilationError.Location.FileName && file == CompilationError.Location.MarkupFile)
                return;

            var oldFileName = CompilationError.Location.FileName;
            CompilationError = CompilationError with { Location = CompilationError.Location with { FileName = fileName, MarkupFile = file } };
            
            // also change other diagnostics, if they were from the same file name
            for (int i = 0; i < OtherDiagnostics.Count; i++)
            {
                var d = OtherDiagnostics[i];
                if (d.Location.FileName == oldFileName)
                    OtherDiagnostics[i] = d with { Location = d.Location with { FileName = fileName, MarkupFile = file } };
            }
        }
    }
}
