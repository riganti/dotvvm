using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using DotVVM.Framework.Hosting.ErrorPages;
using DotVVM.Framework.ResourceManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ErrorPageTemplate : IErrorWriter
    {
        private readonly StringBuilder builder = new StringBuilder();

        public const string InternalCssResourceName = "DotVVM.Framework.Resources.Styles.DotVVM.Internal.css";
        public const string ErrorPageJsResourceName = "DotVVM.Framework.Resources.Scripts.DotVVM.ErrorPage.js";

        public ErrorPageTemplate(
            int errorCode,
            string errorDescription,
            string summary,
            IErrorSectionFormatter[] formatters)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
            Summary = summary;
            Formatters = formatters;
        }

        public int ErrorCode { get; }
        public string ErrorDescription { get; }
        public string Summary { get; }
        public IErrorSectionFormatter[] Formatters { get; }

        public void WriteText(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            Write(WebUtility.HtmlEncode(str));
        }

        public void WriteUnencoded(string? str)
        {
            Write(str);
        }

        public string TransformText()
        {
            // head
            Write(
$@"<!DOCTYPE html>
<html>
    <head>
        <title>Server Error in Application</title>
        <meta charset=UTF-8 />
        <style type=text/css>
");
            using (var cssStream = typeof(DotvvmConfiguration).Assembly.GetManifestResourceStream(InternalCssResourceName))
            using (var cssReader = new StreamReader(cssStream))
            {
                WriteLine(cssReader.ReadToEnd());
            }

            foreach (var f in Formatters)
            {
                WriteLine($"#menu_radio_{f.Id}:checked ~ #container_{f.Id} {{ display: block; }}");
                WriteLine($"#menu_radio_{f.Id}:checked ~ label[for='menu_radio_{f.Id}'] {{ background-color: #2980b9; }}");
                f.WriteStyle(this);
            }
            Write(
@"
        </style>
    </head>
");

            // body
            WriteUnencoded(
$@"
    <body>
        <div class=header-toolbox>
            <button type=button id=save-and-share-button class=execute title='Saves the error as HTML so you can share it with your coworkers'>Save and Share</button>
        </div>
        <h1>Server Error, HTTP {ErrorCode}: {WebUtility.HtmlEncode(ErrorDescription)}</h1>
        <p class=summary>{WebUtility.HtmlEncode(Summary)}</p>
        <hr />
        <div>
");
            foreach (var f in Formatters)
            {
                var checkedStr = f == Formatters[0] ? "checked='checked'" : "";
                WriteUnencoded(
$@"
            <input type=radio id='menu_radio_{f.Id}' class=collapse name=menu {checkedStr} />
            <label for='menu_radio_{f.Id}' class=nav>
                {f.DisplayName}
            </label>
");
            }
            WriteLine("<hr />");
            foreach (var f in Formatters)
            {
                WriteLine($"<div class=container id='container_{f.Id}'>");
                f.WriteBody(this);
                WriteLine("</div>");
            }

            Write(
@"
        </div>

        <p>&nbsp;</p>
        <script>
");
        using (var jsStream = typeof(DotvvmConfiguration).Assembly.GetManifestResourceStream(ErrorPageJsResourceName))
        using (var jsReader = new StreamReader(jsStream))
        {
            WriteLine(jsReader.ReadToEnd());
        }

        Write(@"
        </script>
    </body>
</html>
");
            return builder.ToString();
        }

        public void ObjectBrowser(object? obj)
        {
            var settings = new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = {
                new ReflectionTypeJsonConverter(),
                new ReflectionAssemblyJsonConverter()
            },
                // suppress any errors that occur during serialization (getters may throw exception, ...)
                Error = (sender, args) => {
                    args.ErrorContext.Handled = true;
                }
            };
            var jobject = JObject.FromObject(obj, JsonSerializer.Create(settings));
            ObjectBrowser(jobject);
        }

        public void ObjectBrowser(JArray arr)
        {
            if (arr.Count == 0)
            {
                WriteText(arr.ToString());
            }
            else
            {
                Write(@"
    <div class='object-browser code'>
        <label>
        <input type='checkbox' class='collapse' />
        <span class='collapse-off'>&gt; [ ... ] </span>
        <span class='collapse-on'>[
            <div class='object-arr'> ");
                foreach (var p in arr)
                {
                    if (p is JObject)
                    {
                        ObjectBrowser((JObject)p);
                    }
                    else if (p is JArray)
                    {
                        ObjectBrowser((JArray)p);
                    }
                    else
                    {
                        WriteText(p.ToString());
                    }
                }
                Write(@"</div>]</span></div>");
            }
        }

        public void ObjectBrowser(JObject obj)
        {
            if (obj.Count == 0)
            {
                WriteText(obj.ToString());
            }
            else
            {
                Write(@"<div class='object-browser code'>
        <label>
        <input type='checkbox' class='collapse' />
        <span class='collapse-off'>&gt; { ... } </span>
        <span class='collapse-on'>{
            <div class='object-obj'>");
                foreach (var p in obj)
                {
                    Write("<div class='prop'><span class='propname'>");
                    WriteText(p.Key);
                    Write("</span>:");
                    if (p.Value is JObject)
                    {
                        ObjectBrowser((JObject)p.Value);
                    }
                    else if (p.Value is JArray)
                    {
                        ObjectBrowser((JArray)p.Value);
                    }
                    else
                    {
                        WriteText(p.Value.ToString(Formatting.None));
                    }
                    Write("</div>");
                }
                Write("</div>}</span></div>");
            }
        }


        public void WriteSourceCode(SourceModel source, bool collapse = true)
        {
            Write(@"
    <div class='source code'>
        <label>");
            if (collapse) { Write(@"<input type='checkbox' class='collapse' />"); }
            Write(@"<div class='source-prelines collapse-on'>");
            if (source.PreLines != null) WriteSourceLines(source.LineNumber - source.PreLines.Length, source.PreLines);
            Write(@"</div><div class='source-errorLine'>");
            if (source.CurrentLine != null) WriteErrorLine(source.LineNumber, source.CurrentLine, source.ErrorColumn, source.ErrorLength);
            Write(@"</div><div class='source-postlines collapse-on'>");
            if (source.PostLines != null) WriteSourceLines(source.LineNumber + 1, source.PostLines);
            Write(@"</div></label></div>");
            if (!string.IsNullOrEmpty(source.FileName))
            {
                Write("<p class='source file'>Source File: <strong>");
                WriteText(source.SystemFileName);
                Write($"</strong> +{source.LineNumber}</p>");
            }
        }

        private void WriteSourceLines(int startLine, params string[] lines)
        {
            Write("<pre>");
            for (var i = 0; i < lines.Length; i++)
            {
                Write("<span class='lineNumber'>");
                Write(startLine + i + ": ");
                Write("</span><span class='codeLine'>");
                WriteText(lines[i]);
                WriteLine("</span>");
            }
            Write("</pre>");
        }

        private void WriteErrorLine(int lineNumber, string line, int errorColumn, int errorLength)
        {
            if (errorColumn >= line.Length)
            {
                errorColumn = line.Length - 1;
                errorLength = 0;
                if (errorColumn < 0)
                {
                    errorColumn = 0;
                }
            }
            Write("<pre>");
            Write("<span class='lineNumber'>");
            Write(lineNumber + ": ");
            Write("</span><span class='codeLine'>");
            Write(WebUtility.HtmlEncode(errorColumn == 0 ? "" : line.Remove(errorColumn)));

            var errorUnderline = WebUtility.HtmlEncode(line.Substring(errorColumn, Math.Min(line.Length - errorColumn, errorLength)));
            if (!string.IsNullOrWhiteSpace(errorUnderline))
            {
                Write("<span class='errorUnderline'>");
                Write(errorUnderline);
                Write("</span>");
            }

            Write(WebUtility.HtmlEncode(line.Substring(Math.Min(line.Length, errorColumn + errorLength))));
            WriteLine("</span>");
            Write("</pre>");
        }

        public void WriteKVTable<K, V>(IEnumerable<KeyValuePair<K, V>> table, string className = "")
        {
            Write($@"
    <table class='kvtable {className}'>
        <thead>
        <tr>
            <th> Variable </th>
            <th> Value </th>
        </tr>
        </thead>
        <tbody>");
            foreach (var kvp in table)
            {
                Write("<tr><td>");
                WriteObject(kvp.Key);
                Write("</td><td>");
                WriteObject(kvp.Value);
                WriteLine("</td></tr>");
            }
            Write("</tbody></table>");
        }

        public void WriteObject(object? obj)
        {
            if (obj is IEnumerable<string>)
                WriteText(string.Concat((IEnumerable<string>)obj));
            else
                WriteText(Convert.ToString(obj));
        }

        private void Write(string? textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            builder.Append(textToAppend);
        }

        private void WriteLine(string textToAppend)
        {
            Write(textToAppend);
            builder.AppendLine();
        }
    }
}
