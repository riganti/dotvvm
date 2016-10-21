using DotVVM.Framework.Hosting.ErrorPages;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ErrorPageTemplate: IErrorWriter
    {

	public int ErrorCode { get; set; }

	public string ErrorDescription { get; set; }

	public string Summary { get; set; }

	public IErrorSectionFormatter[] Formatters { get; set; }

	public void WriteText(string str) {
		Write(WebUtility.HtmlEncode(str));
	}

	public void WriteUnencoded(string str) {
		Write(str);
	}

	private void WriteException(Exception ex) 
	{
		using (var sr = new StringReader(ex.ToString()))
		{
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				this.Write(WebUtility.HtmlEncode(line));
				this.Write("<br />");
			}
		}
	}

	public void ObjectBrowser(object obj)
	{
		var jobject = JObject.FromObject(obj);
		ObjectBrowser(jobject);
	}

	public void ObjectBrowser(JArray arr)
	{
		this.Write(@"
		<div class='object-browser code'>
			<label>
			<input type='checkbox' class='collapse' />
			<span class='collapse-off'>&gt; { ... } </span>
			<span class='collapse-on'>[
				<div class='object-arr'> ");
					foreach(var p in arr) {
						if(p is JObject) {
							ObjectBrowser((JObject)p);
						} else if (p is JArray) {
							ObjectBrowser((JArray)p);
						} else {
							this.Write(p.ToString());
						}
					}
			
			this.Write(@"</div>]</span></div>");
	}

	public void ObjectBrowser(JObject obj)
	{
		this.Write(@"<div class='object-browser code'>
			<label>
			<input type='checkbox' class='collapse' />
			<span class='collapse-off'>&gt; { ... } </span>
			<span class='collapse-on'>{
				<div class='object-obj'>");
					foreach(var p in obj) {
						Write("<div class='prop'><span class='propname'>");
						this.WriteText(p.Key);
						Write("</span>:");
						if(p.Value is JObject) {
							ObjectBrowser((JObject)p.Value);
						} else if (p.Value is JArray) {
							ObjectBrowser((JArray)p.Value);
						} else {
							this.WriteText(p.Value.ToString());
						}
						this.Write("</div>");
					}
		this.Write("</div>}</span></div>");
	}


	public void WriteSourceCode(SourceModel source, bool collapse = true)
	{
		this.Write(@"
		<div class='source code'>
			<label>");
				if(collapse){ this.Write(@"<input type='checkbox' class='collapse' />"); }
				Write(@"<div class='source-prelines collapse-on'>");
				if(source.PreLines != null) WriteSourceLines(source.LineNumber - source.PreLines.Length, source.PreLines);
				Write(@"</div><div class='source-errorLine'>");
				if(source.CurrentLine != null) WriteErrorLine(source.LineNumber, source.CurrentLine, source.ErrorColumn, source.ErrorLength);
				this.Write(@"</div><div class='source-postlines collapse-on'>");
				if(source.PostLines != null) WriteSourceLines(source.LineNumber + 1, source.PostLines);
	    Write(@"</div></label></div>");
		if (!string.IsNullOrEmpty(source.FileName)) {
			Write("<p class='source file'>Source File: <strong>");
			Write(source.SystemFileName);
			Write("</strong></p>");
		}
	}

    private void WriteSourceLines(int startLine, params string[] lines)
	{
		Write("<pre>");
		for(var i = 0; i < lines.Length; i++)
		{
			Write("<span class='lineNumber'>");
			Write(startLine + i + ": ");
			Write("</span><span class='codeLine'>");
			Write(WebUtility.HtmlEncode(lines[i]));
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

	    var errorUnderline =WebUtility.HtmlEncode(line.Substring(errorColumn, Math.Min(line.Length - errorColumn, errorLength)));
	    if (!string.IsNullOrWhiteSpace(errorUnderline))
	    {
	        Write("<span class='errorUnderline'>");
			Write(errorUnderline );
			Write("</span>");    
	    }
		
		
		Write(WebUtility.HtmlEncode(line.Substring(Math.Min(line.Length, errorColumn + errorLength))));
		WriteLine("</span>");
		Write("</pre>");
	}

	public void WriteKVTable(IEnumerable keys, IEnumerable values)
	{
		var zip = keys.Cast<object>().Zip(values.Cast<object>(), (k, v) => new KeyValuePair<object, object>(k, v));

		Write(@"
		<table class='kvtable'>
			<tr>
				<th> Variable </th>
				<th> Value </th>
			</tr>");
		foreach(var kvp in zip) {
			Write("<tr><td>");
			WriteObject(kvp.Key);
			Write("</td><td>");
			WriteObject(kvp.Value);
			Write("</td></tr>");
		}
		Write("</table>");
	}

	public void WriteObject(object obj) 
	{
		if (obj is IEnumerable<string>) WriteText(string.Concat((IEnumerable<string>)obj));
		else WriteText(Convert.ToString(obj)); 
	}

    class SourceLine
    {
        public int? LineNumber { get; set; }

        public string Text { get; set; }
	}

        private System.Text.StringBuilder __sb;

        private void Write(string text) {
            __sb.Append(text);
        }

        private void WriteLine(string text) {
            __sb.AppendLine(text);
        }

        public string TransformText()
        {
            __sb = new System.Text.StringBuilder();
__sb.Append(@"

<!DOCTYPE html>
<html>
	<head>
		<title>Server Error in Application</title>
		<meta charset=""UTF-8"" />
		<style type=""text/css"">
body { font-family: 'Segoe UI',Tahoma,sans-serif; font-size: 11pt; color: #333;}
h1 { font-weight: normal; font-size: 24pt; font-style: italic; color: #A82F23 }
h2 { font-style: normal; font-size: 16pt; font-weight: bold; margin-bottom: 35px; }
h3 { color: #004fbd; font-weight: normal; font-size: 14pt; }
p.summary { color: #004fbd, font-size: 1.5em }
table { border-collapse: separate; border-spacing: 0; margin: 0 0 20px; }
th { vertical-align: bottom; padding: 10px 5px 5px 5px; font-weight: 400; color: #a0a0a0; text-align: left; }
td { padding: 3px 10px; }
th, td { border-right: 1px #ddd solid; border-bottom: 1px #ddd solid; border-left: 1px transparent solid; border-top: 1px transparent solid; box-sizing: border-box; }
th:last-child, td:last-child { border-right: 1px transparent solid; }
pre { font-size: 12pt; margin: 0px; font-family: 'Consolas',monospace; }
.source .source-errorLine { color:  #A82F23; }
.errorUnderline { 
		background-color: #FFF7F7;
		border: 1px solid #FF8888;
		color:#FF0909;
		padding: 4px 0;
}
input.collapse { display: none }
input[type=checkbox].collapse ~ .collapse-on  { display: none; }
input[type=checkbox]:checked.collapse ~ .collapse-on  { display: inherit; }
input[type=checkbox]:checked.collapse ~ .collapse-off  { display: none; }
.lineNumber{
	color:#bababa;
}
label{
}
label.nav {
	display: inline-block;
	padding: 4px 20px;
	font-size: 1.1em;
	color: white;
	cursor:pointer;
	background-color:#bcbcbc;
	/*#25384a*/
}
hr{
	border:none;
	height: 1px; 
	background-color: #bcbcbc;
}
.code { font-family: 'Consolas',monospace; }
.object-obj, .object-arr { padding-left: 10px; }
.container { display: none }
.object-browser .propname { font-weight: bold; }
.docLinks { float: right; }
.source.file{
	margin: 11px 0 5px 0;
    font-size: 12px;
}   
");
foreach(var f in Formatters){__sb.Append(@"
#menu_radio_");
__sb.Append(f.Id);
__sb.Append(@":checked ~ #container_");
__sb.Append(f.Id);
__sb.Append(@" { display: block; }
#menu_radio_");
__sb.Append(f.Id);
__sb.Append(@":checked ~ label[for='menu_radio_");
__sb.Append(f.Id);
__sb.Append(@"'] { background-color: #2980b9; }
");
f.WriteHead(this);__sb.Append(@"
");
}__sb.Append(@"
		</style>
	</head>
	<body>
		<h1>Server Error, HTTP ");
__sb.Append( ErrorCode );
__sb.Append(@": ");
__sb.Append( WebUtility.HtmlEncode(ErrorDescription) );
__sb.Append(@"</h1>
		<p class=""summary"">");
__sb.Append( WebUtility.HtmlEncode(Summary) );
__sb.Append(@"</p>
		<hr />
		<div>
		");
 foreach(var f in Formatters) {__sb.Append(@"
			<input type=""radio"" id=""menu_radio_");
__sb.Append(f.Id);
__sb.Append(@""" class=""collapse"" name=""menu"" ");
__sb.Append( f == Formatters[0] ? "checked='checked'" : "" );
__sb.Append(@" />
			<label for=""menu_radio_");
__sb.Append(f.Id);
__sb.Append(@""" class=""nav"">
				");
__sb.Append(f.DisplayName);
__sb.Append(@"
			</label>
		");
}__sb.Append(@"
		<hr />
		");
foreach(var formatter in Formatters){__sb.Append(@"
		<div class=""container"" id=""container_");
__sb.Append(formatter.Id);
__sb.Append(@""">
			");
formatter.WriteBody(this);__sb.Append(@"
		</div>
		");
}__sb.Append(@"

		</div>

		<p>&nbsp;</p>

	</body>
</html>




");
__sb.Append(@"
");

            return __sb.ToString();
        }
    }
}
