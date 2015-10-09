using DotVVM.Framework.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DotvvmMarkupErrorSection : IErrorSectionFormatter
    {
        public string DisplayName => "Dotvvm markup";

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

            w.Write("<div class='exception'><span class='exceptionType'>");
            w.WriteText(exc.GetType().FullName);
            w.Write("</span><span class='exceptionMessage'>");
            w.WriteText(exc.Message);
            w.Write("</span>");
            if (source != null)
            {
                w.WriteSourceCode(source, false);
            }
            w.Write("</div><hr />");
        }

        public virtual SourceModel ExtractSource(Exception exc)
        {
            if (exc is DotvvmCompilationException)
            {
                var compilationException = (DotvvmCompilationException)exc;
                return ErrorFormatter.LoadSourcePiece(compilationException.FileName, compilationException.LineNumber ?? 0,
                    errorColumn: compilationException.Tokens.First().ColumnNumber,
                    errorLength: compilationException.Tokens.Where(t => t.LineNumber == compilationException.LineNumber).Sum(t => t.Length));
            }
            else if(exc is DotvvmControlException)
            {
                var controlException = (DotvvmControlException)exc;
                return ErrorFormatter.LoadSourcePiece(controlException.FileName, controlException.LineNumber ?? 0);
            }
            return null;
        }

        public void WriteHead(IErrorWriter w)
        {

        }

        public static DotvvmMarkupErrorSection Create(Exception ex)
        {
            var iex = ex;
            while(iex != null)
            {
                if (iex is DotvvmCompilationException || iex is DotvvmControlException) break;
                iex = iex.InnerException;
            }
            if (iex != null) return new DotvvmMarkupErrorSection(ex);
            else return null;
        }
    }
}
