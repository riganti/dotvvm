using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DotvvmMarkupErrorSection : IErrorSectionFormatter
    {
        public string DisplayName => "DotVVM Markup";

        public string Id => "dothtml";

        public Exception Error { get; set; }

        public DotvvmMarkupErrorSection(Exception error)
        {
            Error = error;
        }

        public void WriteBody(IErrorWriter w)
        {
            WriteException(w, Error);
        }

        protected virtual void WriteException(IErrorWriter w, Exception exc)
        {
            if (exc.InnerException != null) WriteException(w, exc.InnerException);

            var source = ExtractSource(exc);

            w.WriteUnencoded("<div class='exception'><span class='exceptionType'>");
            w.WriteText(exc.GetType().FullName);
            w.WriteUnencoded("</span><span class='exceptionMessage'>");
            w.WriteText(exc.Message);
            w.WriteUnencoded("</span>");
            if (source != null)
            {
                w.WriteSourceCode(source, false);
            }
            w.WriteUnencoded("</div><hr />");
        }

        public virtual SourceModel? ExtractSource(Exception exc)
        {
            if (exc is DotvvmCompilationException)
            {
                var compilationException = (DotvvmCompilationException)exc;
                return ExtractSourceFromDotvvmCompilationException(compilationException);
            }
            else if (exc is BindingCompilationException)
            {
                var bce = (BindingCompilationException)exc;
                return ExtractSourceFromBindingCompilationException(bce);
            }
            else if (exc is BindingPropertyException bpe)
            {
                return ExtractSourceFromBindingPropertyException(bpe);
            }
            else if (exc is DotvvmControlException)
            {
                var controlException = (DotvvmControlException)exc;
                return ExtractSourceFromDotvvmControlException(controlException);
            }
            return null;
        }

        private SourceModel? ExtractSourceFromDotvvmCompilationException(DotvvmCompilationException compilationException)
        {
            if (compilationException.Tokens != null && compilationException.Tokens.Any())
            {
                var errorColumn = compilationException.Tokens.FirstOrDefault()?.ColumnNumber ?? 0;
                var errorLength = compilationException.Tokens.Where(t => t.LineNumber == compilationException.LineNumber).Sum(t => t.Length);
                if (compilationException.FileName != null)
                    return ErrorFormatter.LoadSourcePiece(compilationException.FileName, compilationException.LineNumber ?? 0,
                        errorColumn: errorColumn,
                        errorLength: errorLength);
                else
                {
                    var line = string.Concat(compilationException.Tokens.Select(s => s.Text));
                    return CreateAnonymousLine(line, lineNumber: compilationException.Tokens.FirstOrDefault()?.LineNumber ?? 0);
                }
            }
            else if (compilationException.FileName != null)
            {
                return ErrorFormatter.LoadSourcePiece(compilationException.FileName, compilationException.LineNumber ?? 0, errorColumn: compilationException.ColumnNumber ?? 0, errorLength: compilationException.ColumnNumber != null ? 1 : 0);
            }
            return null;
        }

        private SourceModel? ExtractSourceFromBindingCompilationException(BindingCompilationException bce)
        {
            if (bce.Expression != null)
            {
                var first = bce.Tokens.FirstOrDefault()?.StartPosition ?? 0;
                return CreateAnonymousLine(bce.Expression, first, (bce.Tokens.LastOrDefault()?.StartPosition ?? 0) + (bce.Tokens.LastOrDefault()?.Length ?? 0) - first);
            }
            else if (bce.Tokens != null)
            {
                return CreateAnonymousLine(string.Concat(bce.Tokens.Select(t => t.Text)));
            }
            return null;
        }

        private SourceModel ExtractSourceFromDotvvmControlException(DotvvmControlException controlException)
        {
            return ErrorFormatter.LoadSourcePiece(controlException.FileName, controlException.LineNumber ?? 0);
        }

        private SourceModel? ExtractSourceFromBindingPropertyException(BindingPropertyException exception)
        {
            var location = exception.Binding.GetProperty<LocationInfoBindingProperty>(ErrorHandlingMode.ReturnNull);
            if (location == null)
                return null;
            var colStart = location.Ranges?.FirstOrDefault().start;
            var colEnd = location.Ranges?.LastOrDefault().end;

            return ErrorFormatter.LoadSourcePiece(location.FileName, location.LineNumber, errorColumn: colStart ?? 0, errorLength: colEnd - colStart ?? 0);
        }

        private SourceModel CreateAnonymousLine(string line, int column = 0, int length = -1, int lineNumber = 0)
        {
            if (length < 0) length = line.Length - column + length;
            return new SourceModel
            {
                CurrentLine = line,
                ErrorColumn = column,
                ErrorLength = length,
                PostLines = new string[0],
                PreLines = new string[0],
                LineNumber = lineNumber
            };
        }

        public void WriteStyle(IErrorWriter w)
        { }

        public static DotvvmMarkupErrorSection? Create(Exception ex)
        {
            var exs = ex.AllInnerExceptions();
            var iex =
                exs.OfType<DotvvmCompilationException>().FirstOrDefault() ??
                exs.OfType<BindingPropertyException>()
                   .Where(bpe => bpe.Binding.GetProperty<LocationInfoBindingProperty>(ErrorHandlingMode.ReturnNull)?.FileName is not null)
                   .FirstOrDefault() ??
                exs.OfType<DotvvmControlException>().FirstOrDefault() as Exception;

            if (iex != null) return new DotvvmMarkupErrorSection(ex);
            else return null;
        }
    }
}
