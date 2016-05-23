using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ExceptionSectionFormatter : IErrorSectionFormatter
    {
        public string DisplayName => "Exception";

        public string Id => "exception";

        public ExceptionModel Exception { get; set; }

        public void WriteBody(IErrorWriter w)
        {
            WriteException(w, Exception);
        }

        protected virtual void WriteException(IErrorWriter w, ExceptionModel model)
        {
            if (model.InnerException != null)
            {
                WriteException(w, model.InnerException);
            }
            w.Write("<div class='exception'><span class='exceptionType'>");
            w.WriteText(model.TypeName);
            w.Write("</span><span class='exceptionMessage'>");
            w.WriteText(model.Message);
            w.Write("</span><hr />");
            if (model.AdditionalInfo != null && model.AdditionalInfo.Length > 0)
            {
                w.Write("<div class='exceptionAdditionalInfo'>");
                foreach (var info in model.AdditionalInfo)
                {
                    w.Write("<div> <h3>");
                    w.WriteText(info.Title);
                    w.Write("</h3>");
                    if (info.Objects != null)
                        foreach (var obj in info.Objects)
                        {
                            if (info.Display == ExceptionAdditionalInfo.DisplayMode.ToString)
                            {
                                w.Write("<p>" + WebUtility.HtmlEncode(obj.ToString()) + "</p>");
                            }
                            else if (info.Display == ExceptionAdditionalInfo.DisplayMode.ObjectBrowser)
                            {
                                w.ObjectBrowser(obj);
                            }
                        }
                    w.Write("</div><hr />");
                }
                w.Write("</div>");
            }
            w.ObjectBrowser(model.OriginalException);
            w.Write("<hr /><div class='exceptionStackTrace'>");
            foreach (var frame in model.Stack)
            {
                w.Write("<div class='frame'><span class='method code'>");
                w.WriteText(FormatMethod(frame.Method));
                w.Write(" </span>");
                if (frame.At.FileName != null) w.WriteText(frame.At.FileName + " +" + frame.At.LineNumber);
                w.Write("<span class='docLinks'>");
                foreach (var icon in frame.MoreInfo)
                {
                    w.Write("<a target=\"_blank\" href='" + icon.Link + "'>");
                    w.Write(icon.ContentHtml);
                    w.Write("</a>");
                }
                w.Write("</span>");
                w.WriteSourceCode(frame.At);
                w.Write("</div>");
            }
            w.Write("</div>");
            w.Write("</div>");
        }

        protected virtual string FormatMethod(MethodBase method)
        {
            var sb = new StringBuilder();
            if (method.DeclaringType != null)
            {
                sb.Append(method.DeclaringType.FullName);
                sb.Append(".");
            }
            sb.Append(method.Name);
            if (method.IsGenericMethod)
            {
                sb.Append("<");
                sb.Append(string.Join(", ", method.GetGenericArguments().Select(t => t.Name)));
                sb.Append(">");
            }
            sb.Append("(");
            var f = false;
            foreach (var p in method.GetParameters())
            {
                if (f) sb.Append(", ");
                else f = true;
                sb.Append(p.ParameterType.Name);
                sb.Append(' ');
                sb.Append(p.Name);
            }
            sb.Append(")");
            return sb.ToString();
        }

        public void WriteHead(IErrorWriter w)
        {
            w.Write(@"
.exception .exceptionType { font-weight: bold; }
.exception .exceptionType:after { content: ': '; }
.exception .exceptionType { font-size: 1.5em; font-style: normal; }
.exception .exceptionMessage { font-style: italic; }
.exceptionStackTrace {  }
.exceptionStackTrace .frame { padding: 2px; margin: 0 0 0 30px; border-bottom: 1px #ddd solid; }
.exceptionStackTrace .frame:hover { background-color: #f0f0f0; }
");
        }
    }
}
