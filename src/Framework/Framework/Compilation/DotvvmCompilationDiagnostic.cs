using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Hosting;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Represents a dothtml compilation error or a warning, along with its location. </summary>
    public record DotvvmCompilationDiagnostic: IEquatable<DotvvmCompilationDiagnostic>
    {
        public DotvvmCompilationDiagnostic(
            string message,
            DiagnosticSeverity severity,
            DotvvmCompilationSourceLocation? location,
            IEnumerable<DotvvmCompilationDiagnostic>? notes = null,
            Exception? innerException = null)
        {
            Message = message;
            Severity = severity;
            Location = location ?? DotvvmCompilationSourceLocation.Unknown;
            Notes = notes?.ToImmutableArray() ?? ImmutableArray<DotvvmCompilationDiagnostic>.Empty;
            InnerException = innerException;
        }

        public string Message { get; init; }
        public Exception? InnerException { get; init; }
        public DiagnosticSeverity Severity { get; init; }
        public DotvvmCompilationSourceLocation Location { get; init; }
        public ImmutableArray<DotvvmCompilationDiagnostic> Notes { get; init; }
        /// <summary> Errors with lower number are preferred when selecting the primary fault to the user. When equal, errors are sorted based on the location. 0 is default for semantic errors, 100 for parser errors and 200 for tokenizer errors. </summary>
        public int Priority { get; init; }

        public bool IsError => Severity == DiagnosticSeverity.Error;
        public bool IsWarning => Severity == DiagnosticSeverity.Warning;
        
        public override string ToString() =>
            $"{Severity}: {Message}\n    at {Location?.ToString() ?? "unknown location"}";
    }

    public sealed record DotvvmCompilationSourceLocation
    {
        public string? FileName { get; init; }
        [JsonIgnore]
        public MarkupFile? MarkupFile { get; init; }
        [JsonIgnore]
        public ImmutableArray<TokenBase> Tokens { get; init; }
        public int? LineNumber { get; init; }
        public int? ColumnNumber { get; init; }
        public int LineErrorLength { get; init; }
        [JsonIgnore]
        public DothtmlNode? RelatedSyntaxNode { get; init; }
        [JsonIgnore]
        public ResolvedTreeNode? RelatedResolvedNode { get; init; }
        public DotvvmProperty? RelatedProperty { get; init; }
        public IBinding? RelatedBinding { get; init; }

        public Type? RelatedControlType => this.RelatedResolvedNode?.GetAncestors(true).OfType<ResolvedControl>().FirstOrDefault()?.Metadata.Type;

        public DotvvmCompilationSourceLocation(
            string? fileName,
            MarkupFile? markupFile,
            IEnumerable<TokenBase>? tokens,
            int? lineNumber = null,
            int? columnNumber = null,
            int? lineErrorLength = null)
        {
            this.Tokens = tokens?.ToImmutableArray() ?? ImmutableArray<TokenBase>.Empty;
            if (this.Tokens.Length > 0)
            {
                lineNumber ??= this.Tokens[0].LineNumber;
                columnNumber ??= this.Tokens[0].ColumnNumber;
                lineErrorLength ??= this.Tokens.Where(t => t.LineNumber == lineNumber).Select(t => (int?)(t.ColumnNumber + t.Length)).LastOrDefault() - columnNumber;
            }

            this.MarkupFile = markupFile;
            this.FileName = fileName ?? markupFile?.FileName;
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
            this.LineErrorLength = lineErrorLength ?? 0;
        }

        public DotvvmCompilationSourceLocation(
            IEnumerable<TokenBase> tokens): this(fileName: null, null, tokens) { }
        public DotvvmCompilationSourceLocation(
            DothtmlNode syntaxNode, IEnumerable<TokenBase>? tokens = null)
            : this(fileName: null, null, tokens ?? syntaxNode?.Tokens)
        {
            RelatedSyntaxNode = syntaxNode;
        }
        public DotvvmCompilationSourceLocation(
            ResolvedTreeNode resolvedNode, DothtmlNode? syntaxNode = null, IEnumerable<TokenBase>? tokens = null)
            : this(
                syntaxNode ?? resolvedNode.GetAncestors(true).FirstOrDefault(n => n.DothtmlNode is {})?.DothtmlNode!,
                tokens
            )
        {
            RelatedResolvedNode = resolvedNode;
            if (resolvedNode.GetAncestors().OfType<ResolvedPropertySetter>().FirstOrDefault() is {} property)
                RelatedProperty = property.Property;
        }

        public static readonly DotvvmCompilationSourceLocation Unknown = new(fileName: null, null, null);
        public bool IsUnknown => FileName is null && MarkupFile is null && Tokens.IsEmpty && LineNumber is null && ColumnNumber is null;

        /// <summary> Text of the affected tokens. Consecutive tokens are concatenated - usually, this returns a single element array. </summary>
        public string[] AffectedSpans
        {
            get
            {
                if (Tokens.IsEmpty)
                    return Array.Empty<string>();
                var spans = new List<string> { Tokens[0].Text };
                for (int i = 1; i < Tokens.Length; i++)
                {
                    if (Tokens[i].StartPosition == Tokens[i - 1].EndPosition)
                        spans[spans.Count - 1] += Tokens[i].Text;
                    else
                        spans.Add(Tokens[i].Text);
                }
                return spans.ToArray();
            }
        }

        /// <summary> Ranges of the affected tokens (in UTF-16 codepoint positions). Consecutive rangess are merged - usually, this returns a single element array. </summary>
        public (int start, int end)[] AffectedRanges
        {
            get
            {
                if (Tokens.IsEmpty)
                    return Array.Empty<(int, int)>();
                var ranges = new (int start, int end)[Tokens.Length];
                ranges[0] = (Tokens[0].StartPosition, Tokens[0].EndPosition);
                int ri = 0;
                for (int i = 1; i < Tokens.Length; i++)
                {
                    if (Tokens[i].StartPosition == Tokens[i - 1].EndPosition)
                        ranges[i].end = Tokens[i].EndPosition;
                    else
                    {
                        ri += 1;
                        ranges[ri] = (Tokens[i].StartPosition, Tokens[i].EndPosition);
                    }
                }
                return ranges.AsSpan(0, ri + 1).ToArray();
            }
        }

        public int? EndLineNumber => Tokens.LastOrDefault()?.LineNumber ?? LineNumber;
        public int? EndColumnNumber => (Tokens.LastOrDefault()?.ColumnNumber + Tokens.LastOrDefault()?.Length) ?? ColumnNumber;

        public override string ToString()
        {
            if (IsUnknown)
                return "Unknown location";
            else if (FileName is {} && LineNumber is {})
            {
                // MSBuild-style file location
                return $"{FileName}({LineNumber}{(ColumnNumber is {} ? "," + ColumnNumber : "")})";
            }
            else
            {
                // only position, plus add the affected spans
                var location =
                    LineNumber is {} && ColumnNumber is {} ? $"{LineNumber},{ColumnNumber}: " :
                    LineNumber is {} ? $"{LineNumber}: " :
                    "";
                return $"{location}{string.Join("; ", AffectedSpans)}";
            }
        }

        public DotvvmLocationInfo ToRuntimeLocation() =>
            new DotvvmLocationInfo(
                this.FileName,
                this.AffectedRanges,
                this.LineNumber,
                this.RelatedControlType,
                this.RelatedProperty
            );
    }
}
