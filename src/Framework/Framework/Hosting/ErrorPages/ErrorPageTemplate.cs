using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using DotVVM.Framework.Hosting.ErrorPages;
using DotVVM.Framework.ResourceManagement;
using System.Text;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DotVVM.Framework.Compilation.ControlTree;
using System.Text.Json.Serialization.Metadata;
using System.Text.Encodings.Web;
using FastExpressionCompiler;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ErrorPageTemplate : IErrorWriter
    {
        private readonly StringBuilder builder = new StringBuilder();

        public const string InternalCssResourceName = "DotVVM.Framework.Resources.Styles.DotVVM.Internal.css";
        public const string ErrorPageJsResourceName = "DotVVM.Framework.Resources.Scripts.DotVVM.ErrorPage.js";

        public ErrorPageTemplate(int errorCode,
            string errorDescription,
            string summary,
            IErrorSectionFormatter[] formatters,
            Exception exception,
            IDotvvmRequestContext? context)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
            Summary = summary;
            Formatters = formatters;
            Exception = exception;
            Context = context;
        }

        public int ErrorCode { get; }
        public string ErrorDescription { get; }
        public string Summary { get; }
        public IErrorSectionFormatter[] Formatters { get; }
        public IDotvvmRequestContext? Context { get; }
        public Exception Exception { get; }

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
            using (var cssStream = typeof(DotvvmConfiguration).Assembly.GetManifestResourceStream(InternalCssResourceName)!)
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

            Write(@"
        </style>
");
            
            if (Context != null)
            {
                foreach (var extension in Context.Services.GetServices<IErrorPageExtension>())
                {
                    WriteLine(extension.GetHeadContents(Context, Exception));
                }
            }

            Write(@"
    </head>
");

            // body
            WriteUnencoded(
$@"
    <body>
        <div class=header-toolbox>
            <button type=button id=save-and-share-button class=execute title='Saves the error as HTML so you can share it with your coworkers'>Save and Share</button>
        </div>
        <h1 class=error-text>Server Error, HTTP {ErrorCode}: {WebUtility.HtmlEncode(ErrorDescription)}</h1>
        <pre class=summary>{WebUtility.HtmlEncode(Summary)}</pre>
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
        using (var jsStream = typeof(DotvvmConfiguration).Assembly.GetManifestResourceStream(ErrorPageJsResourceName)!)
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

        internal static JsonNode SerializeObjectForBrowser(object? obj)
        {
            var settings = new JsonSerializerOptions() {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = {
                    new DebugReflectionTypeJsonConverter(),
                    new ReflectionAssemblyJsonConverter(),
                    new ReflectionTypeJsonConverter(),
                    new DotvvmTypeDescriptorJsonConverter<ITypeDescriptor>(),
                    new Controls.DotvvmControlDebugJsonConverter(),
                    new BindingDebugJsonConverter(),
                    new DotvvmPropertyJsonConverter(),
                    new UnsupportedTypeJsonConverterFactory(),
                },
                TypeInfoResolver = new IgnoreUnsupportedResolver(),
            };
            return JsonSerializer.SerializeToNode(obj, settings)!;
        }

        public void ObjectBrowser(object? obj)
        {
            if (obj is null)
            {
                WriteText("null");
                return;
            }

            
            try
            {
                switch (SerializeObjectForBrowser(obj))
                {
                    case JsonObject jobject:
                        ObjectBrowser(jobject);
                        break;
                    case JsonArray jarray:
                        ObjectBrowser(jarray);
                        break;
                    case var node:
                        WriteText(node.ToString());
                        break;
                };
            }
            catch
            {
                try
                {
                    WriteText(obj.ToString());
                }
                catch
                {
                    WriteText("<serialization error>");
                }
            }
        }

        class IgnoreUnsupportedResolver: DefaultJsonTypeInfoResolver
        {
            HashSet<Type> stack = new HashSet<Type>();
            public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                stack.Add(type);
                try
                {
                    var info = base.GetTypeInfo(type, options);
                    foreach (var prop in info!.Properties.ToArray())
                    {
                        if (stack.Contains(prop.PropertyType))
                        {
                            // object currently being resolved -> fine
                        }
                        else
                        {
                            var converter = prop.CustomConverter ?? options.GetConverter(prop.PropertyType);
                            if (converter is null)
                            {
                                info.Properties.Remove(prop);
                            }
                        }
                    }
                    return info;
                }
                finally
                {
                    stack.Remove(type);
                }
            }
        }

        public void ObjectBrowser(JsonArray arr)
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
                    if (p is JsonObject pObj)
                    {
                        ObjectBrowser(pObj);
                    }
                    else if (p is JsonArray pArr)
                    {
                        ObjectBrowser(pArr);
                    }
                    else if (p is null)
                    {
                        WriteText("null");
                    }
                    else
                    {
                        WriteText(p.ToString());
                    }
                }
                Write(@"</div>]</span></div>");
            }
        }

        public void ObjectBrowser(JsonObject obj)
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
                    if (p.Value is null)
                    {
                        WriteText("null");
                    }
                    else if (p.Value is JsonObject pObj)
                    {
                        ObjectBrowser(pObj);
                    }
                    else if (p.Value is JsonArray pArr)
                    {
                        ObjectBrowser(pArr);
                    }
                    else
                    {
                        WriteText(p.Value.ToJsonString(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
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
                Write($"</strong>:{source.LineNumber}</p>");
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
            }
            if (errorColumn < 0)
            {
                errorColumn = 0;
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

        class UnsupportedTypeJsonConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert) =>
                typeToConvert.IsDelegate() || typeof(ICustomAttributeProvider).IsAssignableFrom(typeToConvert);
            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
                (JsonConverter?)Activator.CreateInstance(typeof(Inner<>).MakeGenericType([ typeToConvert ]));

            class Inner<T> : JsonConverter<T>
            {
                public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
                public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
                {
                    if (value is null)
                        writer.WriteNullValue();
                    else if (value is Delegate)
                        writer.WriteStringValue($"[delegate {value.GetType().ToCode()}]");
                    else
                        writer.WriteStringValue(value.ToString());
                }
            }
        }
    }
}
