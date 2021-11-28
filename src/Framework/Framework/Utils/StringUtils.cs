using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using FastExpressionCompiler;

namespace DotVVM.Framework.Utils
{
    public static class StringUtils
    {
        public static string LimitLength(this string source, int length, string ending = "...")
        {
            if (length < source.Length)
            {
                return source.Substring(0, length - ending.Length) + ending;
            }
            else
            {
                return source;
            }
        }

        internal static string DotvvmInternString(this char ch, string? str = null) =>
            ch switch {
                ' ' => " ",
                '!' => "!",
                '"' => "\"",
                '#' => "#",
                '$' => "$",
                '%' => "%",
                '&' => "&",
                '\'' => "'",
                '(' => "(",
                ')' => ")",
                '*' => "*",
                '+' => "+",
                ',' => ",",
                '-' => "-",
                '.' => ".",
                '/' => "/",
                '0' => "0",
                '1' => "1",
                '2' => "2",
                '3' => "3",
                '4' => "4",
                '5' => "5",
                '6' => "6",
                '7' => "7",
                '8' => "8",
                '9' => "9",
                ':' => ":",
                ';' => ";",
                '<' => "<",
                '=' => "=",
                '>' => ">",
                '?' => "?",
                '@' => "@",
                'A' => "A",
                'B' => "B",
                'C' => "C",
                'D' => "D",
                'E' => "E",
                'F' => "F",
                'G' => "G",
                'H' => "H",
                'I' => "I",
                'J' => "J",
                'K' => "K",
                'L' => "L",
                'M' => "M",
                'N' => "N",
                'O' => "O",
                'P' => "P",
                'Q' => "Q",
                'R' => "R",
                'S' => "S",
                'T' => "T",
                'U' => "U",
                'V' => "V",
                'W' => "W",
                'X' => "X",
                'Y' => "Y",
                'Z' => "Z",
                '[' => "[",
                '\\' => "\\",
                ']' => "]",
                '^' => "^",
                '_' => "_",
                '`' => "`",
                'a' => "a",
                'b' => "b",
                'c' => "c",
                'd' => "d",
                'e' => "e",
                'f' => "f",
                'g' => "g",
                'h' => "h",
                'i' => "i",
                'j' => "j",
                'k' => "k",
                'l' => "l",
                'm' => "m",
                'n' => "n",
                'o' => "o",
                'p' => "p",
                'q' => "q",
                'r' => "r",
                's' => "s",
                't' => "t",
                'u' => "u",
                'v' => "v",
                'w' => "w",
                'x' => "x",
                'y' => "y",
                'z' => "z",
                '{' => "{",
                '|' => "|",
                '}' => "}",
                '~' => "~",
                '\n' => "\n",
                // although .NET intern is quite slow, there is just no reason to have multiple instances of the same character
                _ => string.Intern(str ?? ch.ToString())
            };

        internal static string DotvvmInternString(this string str) =>
            str.AsSpan().DotvvmInternString(str);

        public static void GenerateCode()
        {
            // just run dotnet dump and then `dumpheap -stat -strings` to see what is there suspiciously often...
            var strings = new List<string> {
                // common tag prefixes
                "dot", "bp", "bs", "dc", "cc",

                // some JS fragment occuring often
                "Items()?.length", ".Items()?.length", ".$index()", "$index()", "value", "dotvvm.postBack(", "ko.pureComputed(()=>", "viewModel", "dotvvm.evaluator.wrapObservable(()=>", "$type", "$data", ".$data", "(()=>{let vm=", ",[],", "dotvvm.globalize.bindingNumberToString(", ".Name", ".Id", "(i)=>ko.unwrap(i).Id()", "(i)=>ko.unwrap(i).Text()", "(i)=>ko.unwrap(i).Name()", "(async ()=>{let vm=", ".Visible", ".Enabled", "ko.contextFor(", "import", "service",

                // bindings
                "value", "resource", "command", "controlCommand", "staticCommand", "staticCommand",

                // html attributes
                "accept", "accept-charset", "accesskey", "action", "align", "allow", "alt", "async", "autocapitalize", "autocomplete", "autofocus", "autoplay", "background", "bgcolor", "border", "buffered", "capture", "challenge", "charset", "checked", "cite", "class", "code", "codebase", "color", "cols", "colspan", "content", "contenteditable", "contextmenu", "controls", "coords", "crossorigin", "csp", "data", "datetime", "decoding", "default", "defer", "dir", "dirname", "disabled", "download", "draggable", "enctype", "enterkeyhint", "for", "form", "formaction", "formenctype", "formmethod", "formnovalidate", "formtarget", "headers", "height", "hidden", "high", "href", "hreflang", "http-equiv", "icon", "id", "importance", "integrity", "intrinsicsize", "inputmode", "ismap", "itemprop", "keytype", "kind", "label", "lang", "language", "loading", "list", "loop", "low", "manifest", "max", "maxlength", "minlength", "media", "method", "min", "multiple", "muted", "name", "novalidate", "open", "optimum", "pattern", "ping", "placeholder", "poster", "preload", "radiogroup", "readonly", "referrerpolicy", "rel", "required", "reversed", "rows", "rowspan", "sandbox", "scope", "scoped", "selected", "shape", "size", "sizes", "slot", "span", "spellcheck", "src", "srcdoc", "srclang", "srcset", "start", "step", "style", "summary", "tabindex", "target", "title", "translate", "type", "usemap", "value", "width", "wrap",

                "data-ui", "data-bind",
                "a", "abbr", "acronym", "address", "applet", "area", "article", "aside", "audio", "b", "base", "basefont", "bdi", "bdo", "big", "blockquote", "body", "br", "button", "canvas", "caption", "center", "cite", "code", "col", "colgroup", "data", "datalist", "dd", "del", "details", "dfn", "dialog", "dir", "div", "dl", "dt", "em", "embed", "fieldset", "figcaption", "figure", "font", "footer", "form", "frame", "frameset", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hr", "html", "i", "iframe", "img", "input", "ins", "kbd", "label", "legend", "li", "link", "main", "map", "mark", "meta", "meter", "nav", "noframes", "noscript", "object", "ol", "optgroup", "option", "output", "p", "param", "picture", "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp", "script", "section", "select", "small", "source", "span", "strike", "strong", "style", "sub", "summary", "sup", "svg", "table", "tbody", "td", "template", "textarea", "tfoot", "th", "thead", "time", "title", "tr", "track", "tt", "u", "ul", "var", "video", "wbr",

                // random fragments

                "http://www.w3.org/1999/xhtml",

                "Load", "Init", "PreRender", "ToString", "Equals", "Context", "DotVVM", "en",
                "_this", "_parent", "_control", "_root",
                "PostBack.Handlers",

                "true", "false",
                "  ", "\n\n", "\r\n", "\r\n    ", "\n   ", "\r\n        ", "\n       ", "\r\n            ", "\n           ",
            };

            strings.AddRange(Enumerable.Range(0, 20).Select(x => "c" + x));

            strings.AddRange(DotvvmProperty.AllProperties.Select(x => x.Name));
            strings.AddRange(DotvvmProperty.AllProperties.Select(x => x.DeclaringType.Name));

            var lenGroups = strings.Where(l => l.Length > 1).Distinct().GroupBy(x => x.Length).OrderBy(k => k.Key);
            Console.WriteLine("switch (span.Length)\n{");
            foreach (var lenGroup in lenGroups)
            {
                Console.WriteLine($"    case {lenGroup.Key}:");
                Console.WriteLine( "        switch (ch)");
                Console.WriteLine( "        {");
                var gs = lenGroup.GroupBy(x => x[0]).OrderBy(k => k.Key);
                foreach (var g in gs)
                {
                    var ch = g.Key switch {
                        '\r' => "\\r",
                        '\n' => "\\n",
                        '\t' => "\\t",
                        '\\' => "\\\\",
                        '\'' => "\\'",
                        _ => g.Key.ToString()
                    };
                    Console.WriteLine($"            case '{ch}':");
                    foreach (var str in g)
                    {
                        Console.WriteLine($"                if (span.SequenceEqual({str.ToCode()}.AsSpan()))");
                        Console.WriteLine($"                    return {str.ToCode()};");
                    }
                    Console.WriteLine($"                break;");
                }
                Console.WriteLine( "        }");
                Console.WriteLine( "        break;");
            }
            Console.WriteLine("}");
        }
        internal static string DotvvmInternString(this Span<char> span, string? str = null, bool trySystemIntern = false) =>
            ((ReadOnlySpan<char>)span).DotvvmInternString(str, trySystemIntern);
        internal static string DotvvmInternString(this ReadOnlySpan<char> span, string? str = null, bool trySystemIntern = false)
        {
            if (span.Length == 0)
                return "";

            var ch = span[0];

            if (span.Length == 1)
                return ch.DotvvmInternString();
switch (span.Length)
{
    case 2:
        switch (ch)
        {
            case '\n':
                if (span.SequenceEqual("\n\n".AsSpan()))
                    return "\n\n";
                break;
            case '\r':
                if (span.SequenceEqual("\r\n".AsSpan()))
                    return "\r\n";
                break;
            case ' ':
                if (span.SequenceEqual("  ".AsSpan()))
                    return "  ";
                break;
            case 'I':
                if (span.SequenceEqual("ID".AsSpan()))
                    return "ID";
                break;
            case 'O':
                if (span.SequenceEqual("Ok".AsSpan()))
                    return "Ok";
                break;
            case 'b':
                if (span.SequenceEqual("bp".AsSpan()))
                    return "bp";
                if (span.SequenceEqual("bs".AsSpan()))
                    return "bs";
                if (span.SequenceEqual("br".AsSpan()))
                    return "br";
                break;
            case 'c':
                if (span.SequenceEqual("cc".AsSpan()))
                    return "cc";
                if (span.SequenceEqual("c0".AsSpan()))
                    return "c0";
                if (span.SequenceEqual("c1".AsSpan()))
                    return "c1";
                if (span.SequenceEqual("c2".AsSpan()))
                    return "c2";
                if (span.SequenceEqual("c3".AsSpan()))
                    return "c3";
                if (span.SequenceEqual("c4".AsSpan()))
                    return "c4";
                if (span.SequenceEqual("c5".AsSpan()))
                    return "c5";
                if (span.SequenceEqual("c6".AsSpan()))
                    return "c6";
                if (span.SequenceEqual("c7".AsSpan()))
                    return "c7";
                if (span.SequenceEqual("c8".AsSpan()))
                    return "c8";
                if (span.SequenceEqual("c9".AsSpan()))
                    return "c9";
                break;
            case 'd':
                if (span.SequenceEqual("dc".AsSpan()))
                    return "dc";
                if (span.SequenceEqual("dd".AsSpan()))
                    return "dd";
                if (span.SequenceEqual("dl".AsSpan()))
                    return "dl";
                if (span.SequenceEqual("dt".AsSpan()))
                    return "dt";
                break;
            case 'e':
                if (span.SequenceEqual("em".AsSpan()))
                    return "em";
                if (span.SequenceEqual("en".AsSpan()))
                    return "en";
                break;
            case 'h':
                if (span.SequenceEqual("h1".AsSpan()))
                    return "h1";
                if (span.SequenceEqual("h2".AsSpan()))
                    return "h2";
                if (span.SequenceEqual("h3".AsSpan()))
                    return "h3";
                if (span.SequenceEqual("h4".AsSpan()))
                    return "h4";
                if (span.SequenceEqual("h5".AsSpan()))
                    return "h5";
                if (span.SequenceEqual("h6".AsSpan()))
                    return "h6";
                if (span.SequenceEqual("hr".AsSpan()))
                    return "hr";
                break;
            case 'i':
                if (span.SequenceEqual("id".AsSpan()))
                    return "id";
                break;
            case 'l':
                if (span.SequenceEqual("li".AsSpan()))
                    return "li";
                break;
            case 'o':
                if (span.SequenceEqual("ol".AsSpan()))
                    return "ol";
                break;
            case 'r':
                if (span.SequenceEqual("rp".AsSpan()))
                    return "rp";
                if (span.SequenceEqual("rt".AsSpan()))
                    return "rt";
                break;
            case 't':
                if (span.SequenceEqual("td".AsSpan()))
                    return "td";
                if (span.SequenceEqual("th".AsSpan()))
                    return "th";
                if (span.SequenceEqual("tr".AsSpan()))
                    return "tr";
                if (span.SequenceEqual("tt".AsSpan()))
                    return "tt";
                break;
            case 'u':
                if (span.SequenceEqual("ul".AsSpan()))
                    return "ul";
                break;
        }
        break;
    case 3:
        switch (ch)
        {
            case '.':
                if (span.SequenceEqual(".Id".AsSpan()))
                    return ".Id";
                break;
            case 'R':
                if (span.SequenceEqual("Row".AsSpan()))
                    return "Row";
                break;
            case 'T':
                if (span.SequenceEqual("Tag".AsSpan()))
                    return "Tag";
                break;
            case 'a':
                if (span.SequenceEqual("alt".AsSpan()))
                    return "alt";
                break;
            case 'b':
                if (span.SequenceEqual("bdi".AsSpan()))
                    return "bdi";
                if (span.SequenceEqual("bdo".AsSpan()))
                    return "bdo";
                if (span.SequenceEqual("big".AsSpan()))
                    return "big";
                break;
            case 'c':
                if (span.SequenceEqual("csp".AsSpan()))
                    return "csp";
                if (span.SequenceEqual("col".AsSpan()))
                    return "col";
                if (span.SequenceEqual("c10".AsSpan()))
                    return "c10";
                if (span.SequenceEqual("c11".AsSpan()))
                    return "c11";
                if (span.SequenceEqual("c12".AsSpan()))
                    return "c12";
                if (span.SequenceEqual("c13".AsSpan()))
                    return "c13";
                if (span.SequenceEqual("c14".AsSpan()))
                    return "c14";
                if (span.SequenceEqual("c15".AsSpan()))
                    return "c15";
                if (span.SequenceEqual("c16".AsSpan()))
                    return "c16";
                if (span.SequenceEqual("c17".AsSpan()))
                    return "c17";
                if (span.SequenceEqual("c18".AsSpan()))
                    return "c18";
                if (span.SequenceEqual("c19".AsSpan()))
                    return "c19";
                break;
            case 'd':
                if (span.SequenceEqual("dot".AsSpan()))
                    return "dot";
                if (span.SequenceEqual("dir".AsSpan()))
                    return "dir";
                if (span.SequenceEqual("del".AsSpan()))
                    return "del";
                if (span.SequenceEqual("dfn".AsSpan()))
                    return "dfn";
                if (span.SequenceEqual("div".AsSpan()))
                    return "div";
                break;
            case 'f':
                if (span.SequenceEqual("for".AsSpan()))
                    return "for";
                break;
            case 'i':
                if (span.SequenceEqual("img".AsSpan()))
                    return "img";
                if (span.SequenceEqual("ins".AsSpan()))
                    return "ins";
                break;
            case 'k':
                if (span.SequenceEqual("kbd".AsSpan()))
                    return "kbd";
                break;
            case 'l':
                if (span.SequenceEqual("low".AsSpan()))
                    return "low";
                break;
            case 'm':
                if (span.SequenceEqual("max".AsSpan()))
                    return "max";
                if (span.SequenceEqual("min".AsSpan()))
                    return "min";
                if (span.SequenceEqual("map".AsSpan()))
                    return "map";
                break;
            case 'n':
                if (span.SequenceEqual("nav".AsSpan()))
                    return "nav";
                break;
            case 'p':
                if (span.SequenceEqual("pre".AsSpan()))
                    return "pre";
                break;
            case 'r':
                if (span.SequenceEqual("rel".AsSpan()))
                    return "rel";
                break;
            case 's':
                if (span.SequenceEqual("src".AsSpan()))
                    return "src";
                if (span.SequenceEqual("sub".AsSpan()))
                    return "sub";
                if (span.SequenceEqual("sup".AsSpan()))
                    return "sup";
                if (span.SequenceEqual("svg".AsSpan()))
                    return "svg";
                break;
            case 'v':
                if (span.SequenceEqual("var".AsSpan()))
                    return "var";
                break;
            case 'w':
                if (span.SequenceEqual("wbr".AsSpan()))
                    return "wbr";
                break;
        }
        break;
    case 4:
        switch (ch)
        {
            case '\n':
                if (span.SequenceEqual("\n   ".AsSpan()))
                    return "\n   ";
                break;
            case ',':
                if (span.SequenceEqual(",[],".AsSpan()))
                    return ",[],";
                break;
            case 'D':
                if (span.SequenceEqual("Date".AsSpan()))
                    return "Date";
                if (span.SequenceEqual("Data".AsSpan()))
                    return "Data";
                break;
            case 'E':
                if (span.SequenceEqual("Edit".AsSpan()))
                    return "Edit";
                break;
            case 'H':
                if (span.SequenceEqual("Html".AsSpan()))
                    return "Html";
                break;
            case 'I':
                if (span.SequenceEqual("Init".AsSpan()))
                    return "Init";
                if (span.SequenceEqual("Item".AsSpan()))
                    return "Item";
                break;
            case 'L':
                if (span.SequenceEqual("Load".AsSpan()))
                    return "Load";
                break;
            case 'M':
                if (span.SequenceEqual("Mode".AsSpan()))
                    return "Mode";
                break;
            case 'N':
                if (span.SequenceEqual("Name".AsSpan()))
                    return "Name";
                break;
            case 'R':
                if (span.SequenceEqual("Row2".AsSpan()))
                    return "Row2";
                break;
            case 'S':
                if (span.SequenceEqual("Size".AsSpan()))
                    return "Size";
                break;
            case 'T':
                if (span.SequenceEqual("Text".AsSpan()))
                    return "Text";
                if (span.SequenceEqual("Trap".AsSpan()))
                    return "Trap";
                if (span.SequenceEqual("Type".AsSpan()))
                    return "Type";
                break;
            case 'a':
                if (span.SequenceEqual("abbr".AsSpan()))
                    return "abbr";
                if (span.SequenceEqual("area".AsSpan()))
                    return "area";
                break;
            case 'b':
                if (span.SequenceEqual("base".AsSpan()))
                    return "base";
                if (span.SequenceEqual("body".AsSpan()))
                    return "body";
                break;
            case 'c':
                if (span.SequenceEqual("cite".AsSpan()))
                    return "cite";
                if (span.SequenceEqual("code".AsSpan()))
                    return "code";
                if (span.SequenceEqual("cols".AsSpan()))
                    return "cols";
                break;
            case 'd':
                if (span.SequenceEqual("data".AsSpan()))
                    return "data";
                break;
            case 'f':
                if (span.SequenceEqual("form".AsSpan()))
                    return "form";
                if (span.SequenceEqual("font".AsSpan()))
                    return "font";
                break;
            case 'h':
                if (span.SequenceEqual("high".AsSpan()))
                    return "high";
                if (span.SequenceEqual("href".AsSpan()))
                    return "href";
                if (span.SequenceEqual("head".AsSpan()))
                    return "head";
                if (span.SequenceEqual("html".AsSpan()))
                    return "html";
                break;
            case 'i':
                if (span.SequenceEqual("icon".AsSpan()))
                    return "icon";
                break;
            case 'k':
                if (span.SequenceEqual("kind".AsSpan()))
                    return "kind";
                break;
            case 'l':
                if (span.SequenceEqual("lang".AsSpan()))
                    return "lang";
                if (span.SequenceEqual("list".AsSpan()))
                    return "list";
                if (span.SequenceEqual("loop".AsSpan()))
                    return "loop";
                if (span.SequenceEqual("link".AsSpan()))
                    return "link";
                break;
            case 'm':
                if (span.SequenceEqual("main".AsSpan()))
                    return "main";
                if (span.SequenceEqual("mark".AsSpan()))
                    return "mark";
                if (span.SequenceEqual("meta".AsSpan()))
                    return "meta";
                break;
            case 'n':
                if (span.SequenceEqual("name".AsSpan()))
                    return "name";
                break;
            case 'o':
                if (span.SequenceEqual("open".AsSpan()))
                    return "open";
                break;
            case 'p':
                if (span.SequenceEqual("ping".AsSpan()))
                    return "ping";
                break;
            case 'r':
                if (span.SequenceEqual("rows".AsSpan()))
                    return "rows";
                if (span.SequenceEqual("ruby".AsSpan()))
                    return "ruby";
                break;
            case 's':
                if (span.SequenceEqual("size".AsSpan()))
                    return "size";
                if (span.SequenceEqual("slot".AsSpan()))
                    return "slot";
                if (span.SequenceEqual("span".AsSpan()))
                    return "span";
                if (span.SequenceEqual("step".AsSpan()))
                    return "step";
                if (span.SequenceEqual("samp".AsSpan()))
                    return "samp";
                break;
            case 't':
                if (span.SequenceEqual("type".AsSpan()))
                    return "type";
                if (span.SequenceEqual("time".AsSpan()))
                    return "time";
                if (span.SequenceEqual("true".AsSpan()))
                    return "true";
                break;
            case 'w':
                if (span.SequenceEqual("wrap".AsSpan()))
                    return "wrap";
                break;
        }
        break;
    case 5:
        switch (ch)
        {
            case '$':
                if (span.SequenceEqual("$type".AsSpan()))
                    return "$type";
                if (span.SequenceEqual("$data".AsSpan()))
                    return "$data";
                break;
            case '.':
                if (span.SequenceEqual(".Name".AsSpan()))
                    return ".Name";
                break;
            case 'A':
                if (span.SequenceEqual("Added".AsSpan()))
                    return "Added";
                break;
            case 'C':
                if (span.SequenceEqual("Click".AsSpan()))
                    return "Click";
                if (span.SequenceEqual("Claim".AsSpan()))
                    return "Claim";
                break;
            case 'D':
                if (span.SequenceEqual("Delay".AsSpan()))
                    return "Delay";
                break;
            case 'F':
                if (span.SequenceEqual("Files".AsSpan()))
                    return "Files";
                break;
            case 'L':
                if (span.SequenceEqual("Label".AsSpan()))
                    return "Label";
                break;
            case 'M':
                if (span.SequenceEqual("Model".AsSpan()))
                    return "Model";
                break;
            case 'R':
                if (span.SequenceEqual("Roles".AsSpan()))
                    return "Roles";
                break;
            case 'V':
                if (span.SequenceEqual("Value".AsSpan()))
                    return "Value";
                break;
            case 'W':
                if (span.SequenceEqual("Width".AsSpan()))
                    return "Width";
                break;
            case '_':
                if (span.SequenceEqual("_this".AsSpan()))
                    return "_this";
                if (span.SequenceEqual("_root".AsSpan()))
                    return "_root";
                break;
            case 'a':
                if (span.SequenceEqual("align".AsSpan()))
                    return "align";
                if (span.SequenceEqual("allow".AsSpan()))
                    return "allow";
                if (span.SequenceEqual("async".AsSpan()))
                    return "async";
                if (span.SequenceEqual("aside".AsSpan()))
                    return "aside";
                if (span.SequenceEqual("audio".AsSpan()))
                    return "audio";
                break;
            case 'c':
                if (span.SequenceEqual("class".AsSpan()))
                    return "class";
                if (span.SequenceEqual("color".AsSpan()))
                    return "color";
                break;
            case 'd':
                if (span.SequenceEqual("defer".AsSpan()))
                    return "defer";
                break;
            case 'e':
                if (span.SequenceEqual("embed".AsSpan()))
                    return "embed";
                break;
            case 'f':
                if (span.SequenceEqual("frame".AsSpan()))
                    return "frame";
                if (span.SequenceEqual("false".AsSpan()))
                    return "false";
                break;
            case 'i':
                if (span.SequenceEqual("ismap".AsSpan()))
                    return "ismap";
                if (span.SequenceEqual("input".AsSpan()))
                    return "input";
                break;
            case 'l':
                if (span.SequenceEqual("label".AsSpan()))
                    return "label";
                break;
            case 'm':
                if (span.SequenceEqual("media".AsSpan()))
                    return "media";
                if (span.SequenceEqual("muted".AsSpan()))
                    return "muted";
                if (span.SequenceEqual("meter".AsSpan()))
                    return "meter";
                break;
            case 'p':
                if (span.SequenceEqual("param".AsSpan()))
                    return "param";
                break;
            case 's':
                if (span.SequenceEqual("scope".AsSpan()))
                    return "scope";
                if (span.SequenceEqual("shape".AsSpan()))
                    return "shape";
                if (span.SequenceEqual("sizes".AsSpan()))
                    return "sizes";
                if (span.SequenceEqual("start".AsSpan()))
                    return "start";
                if (span.SequenceEqual("style".AsSpan()))
                    return "style";
                if (span.SequenceEqual("small".AsSpan()))
                    return "small";
                break;
            case 't':
                if (span.SequenceEqual("title".AsSpan()))
                    return "title";
                if (span.SequenceEqual("table".AsSpan()))
                    return "table";
                if (span.SequenceEqual("tbody".AsSpan()))
                    return "tbody";
                if (span.SequenceEqual("tfoot".AsSpan()))
                    return "tfoot";
                if (span.SequenceEqual("thead".AsSpan()))
                    return "thead";
                if (span.SequenceEqual("track".AsSpan()))
                    return "track";
                break;
            case 'v':
                if (span.SequenceEqual("value".AsSpan()))
                    return "value";
                if (span.SequenceEqual("video".AsSpan()))
                    return "video";
                break;
            case 'w':
                if (span.SequenceEqual("width".AsSpan()))
                    return "width";
                break;
        }
        break;
    case 6:
        switch (ch)
        {
            case '\r':
                if (span.SequenceEqual("\r\n    ".AsSpan()))
                    return "\r\n    ";
                break;
            case '.':
                if (span.SequenceEqual(".$data".AsSpan()))
                    return ".$data";
                break;
            case 'A':
                if (span.SequenceEqual("Append".AsSpan()))
                    return "Append";
                break;
            case 'B':
                if (span.SequenceEqual("Button".AsSpan()))
                    return "Button";
                break;
            case 'C':
                if (span.SequenceEqual("Column".AsSpan()))
                    return "Column";
                if (span.SequenceEqual("Custom".AsSpan()))
                    return "Custom";
                if (span.SequenceEqual("Cancel".AsSpan()))
                    return "Cancel";
                break;
            case 'D':
                if (span.SequenceEqual("DotVVM".AsSpan()))
                    return "DotVVM";
                if (span.SequenceEqual("Dialog".AsSpan()))
                    return "Dialog";
                if (span.SequenceEqual("Device".AsSpan()))
                    return "Device";
                break;
            case 'E':
                if (span.SequenceEqual("Equals".AsSpan()))
                    return "Equals";
                if (span.SequenceEqual("Events".AsSpan()))
                    return "Events";
                break;
            case 'G':
                if (span.SequenceEqual("Global".AsSpan()))
                    return "Global";
                break;
            case 'L':
                if (span.SequenceEqual("Loader".AsSpan()))
                    return "Loader";
                break;
            case 'R':
                if (span.SequenceEqual("Remove".AsSpan()))
                    return "Remove";
                break;
            case 'S':
                if (span.SequenceEqual("Script".AsSpan()))
                    return "Script";
                if (span.SequenceEqual("Styles".AsSpan()))
                    return "Styles";
                break;
            case 'T':
                if (span.SequenceEqual("Target".AsSpan()))
                    return "Target";
                break;
            case 'U':
                if (span.SequenceEqual("Update".AsSpan()))
                    return "Update";
                break;
            case 'V':
                if (span.SequenceEqual("Values".AsSpan()))
                    return "Values";
                break;
            case 'a':
                if (span.SequenceEqual("accept".AsSpan()))
                    return "accept";
                if (span.SequenceEqual("action".AsSpan()))
                    return "action";
                if (span.SequenceEqual("applet".AsSpan()))
                    return "applet";
                break;
            case 'b':
                if (span.SequenceEqual("border".AsSpan()))
                    return "border";
                if (span.SequenceEqual("button".AsSpan()))
                    return "button";
                break;
            case 'c':
                if (span.SequenceEqual("coords".AsSpan()))
                    return "coords";
                if (span.SequenceEqual("canvas".AsSpan()))
                    return "canvas";
                if (span.SequenceEqual("center".AsSpan()))
                    return "center";
                break;
            case 'd':
                if (span.SequenceEqual("dialog".AsSpan()))
                    return "dialog";
                break;
            case 'f':
                if (span.SequenceEqual("figure".AsSpan()))
                    return "figure";
                if (span.SequenceEqual("footer".AsSpan()))
                    return "footer";
                break;
            case 'h':
                if (span.SequenceEqual("height".AsSpan()))
                    return "height";
                if (span.SequenceEqual("hidden".AsSpan()))
                    return "hidden";
                if (span.SequenceEqual("header".AsSpan()))
                    return "header";
                break;
            case 'i':
                if (span.SequenceEqual("import".AsSpan()))
                    return "import";
                if (span.SequenceEqual("iframe".AsSpan()))
                    return "iframe";
                break;
            case 'l':
                if (span.SequenceEqual("legend".AsSpan()))
                    return "legend";
                break;
            case 'm':
                if (span.SequenceEqual("method".AsSpan()))
                    return "method";
                break;
            case 'o':
                if (span.SequenceEqual("object".AsSpan()))
                    return "object";
                if (span.SequenceEqual("option".AsSpan()))
                    return "option";
                if (span.SequenceEqual("output".AsSpan()))
                    return "output";
                break;
            case 'p':
                if (span.SequenceEqual("poster".AsSpan()))
                    return "poster";
                break;
            case 's':
                if (span.SequenceEqual("scoped".AsSpan()))
                    return "scoped";
                if (span.SequenceEqual("srcdoc".AsSpan()))
                    return "srcdoc";
                if (span.SequenceEqual("srcset".AsSpan()))
                    return "srcset";
                if (span.SequenceEqual("script".AsSpan()))
                    return "script";
                if (span.SequenceEqual("select".AsSpan()))
                    return "select";
                if (span.SequenceEqual("source".AsSpan()))
                    return "source";
                if (span.SequenceEqual("strike".AsSpan()))
                    return "strike";
                if (span.SequenceEqual("strong".AsSpan()))
                    return "strong";
                break;
            case 't':
                if (span.SequenceEqual("target".AsSpan()))
                    return "target";
                break;
            case 'u':
                if (span.SequenceEqual("usemap".AsSpan()))
                    return "usemap";
                break;
        }
        break;
    case 7:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("Context".AsSpan()))
                    return "Context";
                if (span.SequenceEqual("Checked".AsSpan()))
                    return "Checked";
                if (span.SequenceEqual("Column2".AsSpan()))
                    return "Column2";
                if (span.SequenceEqual("Command".AsSpan()))
                    return "Command";
                if (span.SequenceEqual("Columns".AsSpan()))
                    return "Columns";
                if (span.SequenceEqual("Changed".AsSpan()))
                    return "Changed";
                if (span.SequenceEqual("Content".AsSpan()))
                    return "Content";
                break;
            case 'D':
                if (span.SequenceEqual("DataSet".AsSpan()))
                    return "DataSet";
                break;
            case 'E':
                if (span.SequenceEqual("Enabled".AsSpan()))
                    return "Enabled";
                if (span.SequenceEqual("Exclude".AsSpan()))
                    return "Exclude";
                break;
            case 'L':
                if (span.SequenceEqual("ListBox".AsSpan()))
                    return "ListBox";
                if (span.SequenceEqual("Literal".AsSpan()))
                    return "Literal";
                break;
            case 'M':
                if (span.SequenceEqual("Message".AsSpan()))
                    return "Message";
                break;
            case 'P':
                if (span.SequenceEqual("Prepend".AsSpan()))
                    return "Prepend";
                break;
            case 'T':
                if (span.SequenceEqual("TextBox".AsSpan()))
                    return "TextBox";
                break;
            case 'U':
                if (span.SequenceEqual("UITests".AsSpan()))
                    return "UITests";
                break;
            case 'V':
                if (span.SequenceEqual("Visible".AsSpan()))
                    return "Visible";
                break;
            case '_':
                if (span.SequenceEqual("_parent".AsSpan()))
                    return "_parent";
                break;
            case 'a':
                if (span.SequenceEqual("acronym".AsSpan()))
                    return "acronym";
                if (span.SequenceEqual("address".AsSpan()))
                    return "address";
                if (span.SequenceEqual("article".AsSpan()))
                    return "article";
                break;
            case 'b':
                if (span.SequenceEqual("bgcolor".AsSpan()))
                    return "bgcolor";
                break;
            case 'c':
                if (span.SequenceEqual("command".AsSpan()))
                    return "command";
                if (span.SequenceEqual("capture".AsSpan()))
                    return "capture";
                if (span.SequenceEqual("charset".AsSpan()))
                    return "charset";
                if (span.SequenceEqual("checked".AsSpan()))
                    return "checked";
                if (span.SequenceEqual("colspan".AsSpan()))
                    return "colspan";
                if (span.SequenceEqual("content".AsSpan()))
                    return "content";
                if (span.SequenceEqual("caption".AsSpan()))
                    return "caption";
                break;
            case 'd':
                if (span.SequenceEqual("default".AsSpan()))
                    return "default";
                if (span.SequenceEqual("dirname".AsSpan()))
                    return "dirname";
                if (span.SequenceEqual("data-ui".AsSpan()))
                    return "data-ui";
                if (span.SequenceEqual("details".AsSpan()))
                    return "details";
                break;
            case 'e':
                if (span.SequenceEqual("enctype".AsSpan()))
                    return "enctype";
                break;
            case 'h':
                if (span.SequenceEqual("headers".AsSpan()))
                    return "headers";
                break;
            case 'k':
                if (span.SequenceEqual("keytype".AsSpan()))
                    return "keytype";
                break;
            case 'l':
                if (span.SequenceEqual("loading".AsSpan()))
                    return "loading";
                break;
            case 'o':
                if (span.SequenceEqual("optimum".AsSpan()))
                    return "optimum";
                break;
            case 'p':
                if (span.SequenceEqual("pattern".AsSpan()))
                    return "pattern";
                if (span.SequenceEqual("preload".AsSpan()))
                    return "preload";
                if (span.SequenceEqual("picture".AsSpan()))
                    return "picture";
                break;
            case 'r':
                if (span.SequenceEqual("rowspan".AsSpan()))
                    return "rowspan";
                break;
            case 's':
                if (span.SequenceEqual("service".AsSpan()))
                    return "service";
                if (span.SequenceEqual("sandbox".AsSpan()))
                    return "sandbox";
                if (span.SequenceEqual("srclang".AsSpan()))
                    return "srclang";
                if (span.SequenceEqual("summary".AsSpan()))
                    return "summary";
                if (span.SequenceEqual("section".AsSpan()))
                    return "section";
                break;
        }
        break;
    case 8:
        switch (ch)
        {
            case '\n':
                if (span.SequenceEqual("\n       ".AsSpan()))
                    return "\n       ";
                break;
            case '$':
                if (span.SequenceEqual("$index()".AsSpan()))
                    return "$index()";
                break;
            case '.':
                if (span.SequenceEqual(".Visible".AsSpan()))
                    return ".Visible";
                if (span.SequenceEqual(".Enabled".AsSpan()))
                    return ".Enabled";
                break;
            case 'C':
                if (span.SequenceEqual("ClientID".AsSpan()))
                    return "ClientID";
                if (span.SequenceEqual("CssClass".AsSpan()))
                    return "CssClass";
                if (span.SequenceEqual("CheckBox".AsSpan()))
                    return "CheckBox";
                if (span.SequenceEqual("ComboBox".AsSpan()))
                    return "ComboBox";
                if (span.SequenceEqual("Control1".AsSpan()))
                    return "Control1";
                if (span.SequenceEqual("Control2".AsSpan()))
                    return "Control2";
                break;
            case 'D':
                if (span.SequenceEqual("DoAction".AsSpan()))
                    return "DoAction";
                break;
            case 'G':
                if (span.SequenceEqual("GridView".AsSpan()))
                    return "GridView";
                break;
            case 'H':
                if (span.SequenceEqual("Handlers".AsSpan()))
                    return "Handlers";
                break;
            case 'I':
                if (span.SequenceEqual("Internal".AsSpan()))
                    return "Internal";
                break;
            case 'M':
                if (span.SequenceEqual("MyButton".AsSpan()))
                    return "MyButton";
                break;
            case 'N':
                if (span.SequenceEqual("NewTitle".AsSpan()))
                    return "NewTitle";
                break;
            case 'P':
                if (span.SequenceEqual("PostBack".AsSpan()))
                    return "PostBack";
                break;
            case 'R':
                if (span.SequenceEqual("ResultId".AsSpan()))
                    return "ResultId";
                if (span.SequenceEqual("RoleView".AsSpan()))
                    return "RoleView";
                if (span.SequenceEqual("Repeater".AsSpan()))
                    return "Repeater";
                break;
            case 'S':
                if (span.SequenceEqual("Suppress".AsSpan()))
                    return "Suppress";
                if (span.SequenceEqual("Selector".AsSpan()))
                    return "Selector";
                break;
            case 'T':
                if (span.SequenceEqual("ToString".AsSpan()))
                    return "ToString";
                if (span.SequenceEqual("Template".AsSpan()))
                    return "Template";
                break;
            case 'U':
                if (span.SequenceEqual("UniqueID".AsSpan()))
                    return "UniqueID";
                break;
            case 'W':
                if (span.SequenceEqual("Wrappers".AsSpan()))
                    return "Wrappers";
                break;
            case '_':
                if (span.SequenceEqual("_control".AsSpan()))
                    return "_control";
                break;
            case 'a':
                if (span.SequenceEqual("autoplay".AsSpan()))
                    return "autoplay";
                break;
            case 'b':
                if (span.SequenceEqual("buffered".AsSpan()))
                    return "buffered";
                if (span.SequenceEqual("basefont".AsSpan()))
                    return "basefont";
                break;
            case 'c':
                if (span.SequenceEqual("codebase".AsSpan()))
                    return "codebase";
                if (span.SequenceEqual("controls".AsSpan()))
                    return "controls";
                if (span.SequenceEqual("colgroup".AsSpan()))
                    return "colgroup";
                break;
            case 'd':
                if (span.SequenceEqual("datetime".AsSpan()))
                    return "datetime";
                if (span.SequenceEqual("decoding".AsSpan()))
                    return "decoding";
                if (span.SequenceEqual("disabled".AsSpan()))
                    return "disabled";
                if (span.SequenceEqual("download".AsSpan()))
                    return "download";
                if (span.SequenceEqual("datalist".AsSpan()))
                    return "datalist";
                break;
            case 'f':
                if (span.SequenceEqual("fieldset".AsSpan()))
                    return "fieldset";
                if (span.SequenceEqual("frameset".AsSpan()))
                    return "frameset";
                break;
            case 'h':
                if (span.SequenceEqual("hreflang".AsSpan()))
                    return "hreflang";
                break;
            case 'i':
                if (span.SequenceEqual("itemprop".AsSpan()))
                    return "itemprop";
                break;
            case 'l':
                if (span.SequenceEqual("language".AsSpan()))
                    return "language";
                break;
            case 'm':
                if (span.SequenceEqual("manifest".AsSpan()))
                    return "manifest";
                if (span.SequenceEqual("multiple".AsSpan()))
                    return "multiple";
                break;
            case 'n':
                if (span.SequenceEqual("noframes".AsSpan()))
                    return "noframes";
                if (span.SequenceEqual("noscript".AsSpan()))
                    return "noscript";
                break;
            case 'o':
                if (span.SequenceEqual("optgroup".AsSpan()))
                    return "optgroup";
                break;
            case 'p':
                if (span.SequenceEqual("progress".AsSpan()))
                    return "progress";
                break;
            case 'r':
                if (span.SequenceEqual("resource".AsSpan()))
                    return "resource";
                if (span.SequenceEqual("readonly".AsSpan()))
                    return "readonly";
                if (span.SequenceEqual("required".AsSpan()))
                    return "required";
                if (span.SequenceEqual("reversed".AsSpan()))
                    return "reversed";
                break;
            case 's':
                if (span.SequenceEqual("selected".AsSpan()))
                    return "selected";
                break;
            case 't':
                if (span.SequenceEqual("tabindex".AsSpan()))
                    return "tabindex";
                if (span.SequenceEqual("template".AsSpan()))
                    return "template";
                if (span.SequenceEqual("textarea".AsSpan()))
                    return "textarea";
                break;
        }
        break;
    case 9:
        switch (ch)
        {
            case '.':
                if (span.SequenceEqual(".$index()".AsSpan()))
                    return ".$index()";
                break;
            case 'C':
                if (span.SequenceEqual("ClaimView".AsSpan()))
                    return "ClaimView";
                break;
            case 'D':
                if (span.SequenceEqual("DataPager".AsSpan()))
                    return "DataPager";
                break;
            case 'E':
                if (span.SequenceEqual("EventName".AsSpan()))
                    return "EventName";
                if (span.SequenceEqual("EditClick".AsSpan()))
                    return "EditClick";
                if (span.SequenceEqual("EmptyData".AsSpan()))
                    return "EmptyData";
                break;
            case 'G':
                if (span.SequenceEqual("GroupName".AsSpan()))
                    return "GroupName";
                break;
            case 'I':
                if (span.SequenceEqual("InnerText".AsSpan()))
                    return "InnerText";
                if (span.SequenceEqual("IsSpaPage".AsSpan()))
                    return "IsSpaPage";
                break;
            case 'O':
                if (span.SequenceEqual("OnCommand".AsSpan()))
                    return "OnCommand";
                break;
            case 'P':
                if (span.SequenceEqual("PreRender".AsSpan()))
                    return "PreRender";
                if (span.SequenceEqual("Parameter".AsSpan()))
                    return "Parameter";
                break;
            case 'R':
                if (span.SequenceEqual("RouteName".AsSpan()))
                    return "RouteName";
                if (span.SequenceEqual("RouteLink".AsSpan()))
                    return "RouteLink";
                break;
            case 'T':
                if (span.SequenceEqual("TextInput".AsSpan()))
                    return "TextInput";
                break;
            case 'U':
                if (span.SequenceEqual("UrlSuffix".AsSpan()))
                    return "UrlSuffix";
                break;
            case 'V':
                if (span.SequenceEqual("Validator".AsSpan()))
                    return "Validator";
                break;
            case 'a':
                if (span.SequenceEqual("accesskey".AsSpan()))
                    return "accesskey";
                if (span.SequenceEqual("autofocus".AsSpan()))
                    return "autofocus";
                break;
            case 'c':
                if (span.SequenceEqual("challenge".AsSpan()))
                    return "challenge";
                break;
            case 'd':
                if (span.SequenceEqual("draggable".AsSpan()))
                    return "draggable";
                if (span.SequenceEqual("data-bind".AsSpan()))
                    return "data-bind";
                break;
            case 'i':
                if (span.SequenceEqual("integrity".AsSpan()))
                    return "integrity";
                if (span.SequenceEqual("inputmode".AsSpan()))
                    return "inputmode";
                break;
            case 'm':
                if (span.SequenceEqual("maxlength".AsSpan()))
                    return "maxlength";
                if (span.SequenceEqual("minlength".AsSpan()))
                    return "minlength";
                break;
            case 't':
                if (span.SequenceEqual("translate".AsSpan()))
                    return "translate";
                break;
            case 'v':
                if (span.SequenceEqual("viewModel".AsSpan()))
                    return "viewModel";
                break;
        }
        break;
    case 10:
        switch (ch)
        {
            case '\r':
                if (span.SequenceEqual("\r\n        ".AsSpan()))
                    return "\r\n        ";
                break;
            case 'B':
                if (span.SequenceEqual("ButtonBase".AsSpan()))
                    return "ButtonBase";
                break;
            case 'D':
                if (span.SequenceEqual("DataSource".AsSpan()))
                    return "DataSource";
                if (span.SequenceEqual("Directives".AsSpan()))
                    return "Directives";
                if (span.SequenceEqual("DotvvmView".AsSpan()))
                    return "DotvvmView";
                if (span.SequenceEqual("DeviceList".AsSpan()))
                    return "DeviceList";
                break;
            case 'F':
                if (span.SequenceEqual("FilesCount".AsSpan()))
                    return "FilesCount";
                if (span.SequenceEqual("FileUpload".AsSpan()))
                    return "FileUpload";
                break;
            case 'H':
                if (span.SequenceEqual("HeaderText".AsSpan()))
                    return "HeaderText";
                break;
            case 'I':
                if (span.SequenceEqual("IsEditable".AsSpan()))
                    return "IsEditable";
                break;
            case 'M':
                if (span.SequenceEqual("MyProperty".AsSpan()))
                    return "MyProperty";
                break;
            case 'O':
                if (span.SequenceEqual("OffCommand".AsSpan()))
                    return "OffCommand";
                break;
            case 'P':
                if (span.SequenceEqual("PrefixText".AsSpan()))
                    return "PrefixText";
                break;
            case 'T':
                if (span.SequenceEqual("TableUtils".AsSpan()))
                    return "TableUtils";
                break;
            case 'V':
                if (span.SequenceEqual("Validation".AsSpan()))
                    return "Validation";
                break;
            case 'b':
                if (span.SequenceEqual("background".AsSpan()))
                    return "background";
                if (span.SequenceEqual("blockquote".AsSpan()))
                    return "blockquote";
                break;
            case 'f':
                if (span.SequenceEqual("formaction".AsSpan()))
                    return "formaction";
                if (span.SequenceEqual("formmethod".AsSpan()))
                    return "formmethod";
                if (span.SequenceEqual("formtarget".AsSpan()))
                    return "formtarget";
                if (span.SequenceEqual("figcaption".AsSpan()))
                    return "figcaption";
                break;
            case 'h':
                if (span.SequenceEqual("http-equiv".AsSpan()))
                    return "http-equiv";
                break;
            case 'i':
                if (span.SequenceEqual("importance".AsSpan()))
                    return "importance";
                break;
            case 'n':
                if (span.SequenceEqual("novalidate".AsSpan()))
                    return "novalidate";
                break;
            case 'r':
                if (span.SequenceEqual("radiogroup".AsSpan()))
                    return "radiogroup";
                break;
            case 's':
                if (span.SequenceEqual("spellcheck".AsSpan()))
                    return "spellcheck";
                break;
        }
        break;
    case 11:
        switch (ch)
        {
            case 'A':
                if (span.SequenceEqual("ArticleBase".AsSpan()))
                    return "ArticleBase";
                break;
            case 'C':
                if (span.SequenceEqual("CheckedItem".AsSpan()))
                    return "CheckedItem";
                if (span.SequenceEqual("Concurrency".AsSpan()))
                    return "Concurrency";
                break;
            case 'D':
                if (span.SequenceEqual("DoubleClick".AsSpan()))
                    return "DoubleClick";
                if (span.SequenceEqual("DataContext".AsSpan()))
                    return "DataContext";
                break;
            case 'H':
                if (span.SequenceEqual("HtmlLiteral".AsSpan()))
                    return "HtmlLiteral";
                break;
            case 'J':
                if (span.SequenceEqual("JsComponent".AsSpan()))
                    return "JsComponent";
                break;
            case 'M':
                if (span.SequenceEqual("MaxFileSize".AsSpan()))
                    return "MaxFileSize";
                break;
            case 'R':
                if (span.SequenceEqual("ReplaceWith".AsSpan()))
                    return "ReplaceWith";
                if (span.SequenceEqual("RadioButton".AsSpan()))
                    return "RadioButton";
                break;
            case 'S':
                if (span.SequenceEqual("SortChanged".AsSpan()))
                    return "SortChanged";
                break;
            case 'c':
                if (span.SequenceEqual("contextmenu".AsSpan()))
                    return "contextmenu";
                if (span.SequenceEqual("crossorigin".AsSpan()))
                    return "crossorigin";
                break;
            case 'f':
                if (span.SequenceEqual("formenctype".AsSpan()))
                    return "formenctype";
                break;
            case 'p':
                if (span.SequenceEqual("placeholder".AsSpan()))
                    return "placeholder";
                break;
        }
        break;
    case 12:
        switch (ch)
        {
            case '\n':
                if (span.SequenceEqual("\n           ".AsSpan()))
                    return "\n           ";
                break;
            case 'A':
                if (span.SequenceEqual("AllowSorting".AsSpan()))
                    return "AllowSorting";
                break;
            case 'C':
                if (span.SequenceEqual("CheckedValue".AsSpan()))
                    return "CheckedValue";
                if (span.SequenceEqual("ClientIDMode".AsSpan()))
                    return "ClientIDMode";
                if (span.SequenceEqual("CheckedItems".AsSpan()))
                    return "CheckedItems";
                break;
            case 'D':
                if (span.SequenceEqual("Dependencies".AsSpan()))
                    return "Dependencies";
                break;
            case 'E':
                if (span.SequenceEqual("EditTemplate".AsSpan()))
                    return "EditTemplate";
                if (span.SequenceEqual("Environments".AsSpan()))
                    return "Environments";
                break;
            case 'F':
                if (span.SequenceEqual("FormatString".AsSpan()))
                    return "FormatString";
                if (span.SequenceEqual("FormControls".AsSpan()))
                    return "FormControls";
                break;
            case 'G':
                if (span.SequenceEqual("GenerateStub".AsSpan()))
                    return "GenerateStub";
                break;
            case 'I':
                if (span.SequenceEqual("ItemTemplate".AsSpan()))
                    return "ItemTemplate";
                if (span.SequenceEqual("InlineScript".AsSpan()))
                    return "InlineScript";
                if (span.SequenceEqual("ItemsControl".AsSpan()))
                    return "ItemsControl";
                if (span.SequenceEqual("InnerWrapper".AsSpan()))
                    return "InnerWrapper";
                break;
            case 'N':
                if (span.SequenceEqual("NamedCommand".AsSpan()))
                    return "NamedCommand";
                break;
            case 'O':
                if (span.SequenceEqual("OnCreateItem".AsSpan()))
                    return "OnCreateItem";
                if (span.SequenceEqual("OuterWrapper".AsSpan()))
                    return "OuterWrapper";
                break;
            case 'P':
                if (span.SequenceEqual("PathFragment".AsSpan()))
                    return "PathFragment";
                if (span.SequenceEqual("PromptButton".AsSpan()))
                    return "PromptButton";
                break;
            case 'R':
                if (span.SequenceEqual("ResourceName".AsSpan()))
                    return "ResourceName";
                break;
            case 'S':
                if (span.SequenceEqual("SelectorBase".AsSpan()))
                    return "SelectorBase";
                if (span.SequenceEqual("SelectorItem".AsSpan()))
                    return "SelectorItem";
                break;
            case 'T':
                if (span.SequenceEqual("TitleBinding".AsSpan()))
                    return "TitleBinding";
                if (span.SequenceEqual("TemplateHost".AsSpan()))
                    return "TemplateHost";
                if (span.SequenceEqual("TextRepeater".AsSpan()))
                    return "TextRepeater";
                break;
            case 'V':
                if (span.SequenceEqual("ValueBinding".AsSpan()))
                    return "ValueBinding";
                break;
            case 'a':
                if (span.SequenceEqual("autocomplete".AsSpan()))
                    return "autocomplete";
                break;
            case 'e':
                if (span.SequenceEqual("enterkeyhint".AsSpan()))
                    return "enterkeyhint";
                break;
        }
        break;
    case 13:
        switch (ch)
        {
            case '(':
                if (span.SequenceEqual("(()=>{let vm=".AsSpan()))
                    return "(()=>{let vm=";
                break;
            case 'A':
                if (span.SequenceEqual("ArticleEditor".AsSpan()))
                    return "ArticleEditor";
                if (span.SequenceEqual("ArticleDetail".AsSpan()))
                    return "ArticleDetail";
                break;
            case 'B':
                if (span.SequenceEqual("ButtonTagName".AsSpan()))
                    return "ButtonTagName";
                if (span.SequenceEqual("ButtonWrapper".AsSpan()))
                    return "ButtonWrapper";
                break;
            case 'C':
                if (span.SequenceEqual("ColumnVisible".AsSpan()))
                    return "ColumnVisible";
                break;
            case 'D':
                if (span.SequenceEqual("DotvvmControl".AsSpan()))
                    return "DotvvmControl";
                break;
            case 'E':
                if (span.SequenceEqual("EmptyItemText".AsSpan()))
                    return "EmptyItemText";
                break;
            case 'H':
                if (span.SequenceEqual("HideWhenValid".AsSpan()))
                    return "HideWhenValid";
                break;
            case 'I':
                if (span.SequenceEqual("InlineEditing".AsSpan()))
                    return "InlineEditing";
                if (span.SequenceEqual("IncludeInPage".AsSpan()))
                    return "IncludeInPage";
                break;
            case 'M':
                if (span.SequenceEqual("MultiSelector".AsSpan()))
                    return "MultiSelector";
                break;
            case 'N':
                if (span.SequenceEqual("NumberBinding".AsSpan()))
                    return "NumberBinding";
                break;
            case 'R':
                if (span.SequenceEqual("RowDecorators".AsSpan()))
                    return "RowDecorators";
                break;
            case 'S':
                if (span.SequenceEqual("SelectedValue".AsSpan()))
                    return "SelectedValue";
                break;
            case 'U':
                if (span.SequenceEqual("UseHistoryApi".AsSpan()))
                    return "UseHistoryApi";
                if (span.SequenceEqual("UploadedFiles".AsSpan()))
                    return "UploadedFiles";
                break;
            case 'i':
                if (span.SequenceEqual("intrinsicsize".AsSpan()))
                    return "intrinsicsize";
                break;
            case 's':
                if (span.SequenceEqual("staticCommand".AsSpan()))
                    return "staticCommand";
                break;
        }
        break;
    case 14:
        switch (ch)
        {
            case '\r':
                if (span.SequenceEqual("\r\n            ".AsSpan()))
                    return "\r\n            ";
                break;
            case 'C':
                if (span.SequenceEqual("ChangedBinding".AsSpan()))
                    return "ChangedBinding";
                if (span.SequenceEqual("CellDecorators".AsSpan()))
                    return "CellDecorators";
                break;
            case 'E':
                if (span.SequenceEqual("ExcludedQueues".AsSpan()))
                    return "ExcludedQueues";
                break;
            case 'F':
                if (span.SequenceEqual("FilterTemplate".AsSpan()))
                    return "FilterTemplate";
                break;
            case 'G':
                if (span.SequenceEqual("GridViewColumn".AsSpan()))
                    return "GridViewColumn";
                break;
            case 'H':
                if (span.SequenceEqual("HtmlCapability".AsSpan()))
                    return "HtmlCapability";
                if (span.SequenceEqual("HeaderCssClass".AsSpan()))
                    return "HeaderCssClass";
                if (span.SequenceEqual("HeaderTemplate".AsSpan()))
                    return "HeaderTemplate";
                break;
            case 'I':
                if (span.SequenceEqual("InnerViewModel".AsSpan()))
                    return "InnerViewModel";
                if (span.SequenceEqual("ItemKeyBinding".AsSpan()))
                    return "ItemKeyBinding";
                if (span.SequenceEqual("IncludedQueues".AsSpan()))
                    return "IncludedQueues";
                if (span.SequenceEqual("IsSubmitButton".AsSpan()))
                    return "IsSubmitButton";
                break;
            case 'M':
                if (span.SequenceEqual("MarkupFileName".AsSpan()))
                    return "MarkupFileName";
                break;
            case 'R':
                if (span.SequenceEqual("RequestContext".AsSpan()))
                    return "RequestContext";
                if (span.SequenceEqual("RenderSettings".AsSpan()))
                    return "RenderSettings";
                break;
            case 'S':
                if (span.SequenceEqual("SetToolTipText".AsSpan()))
                    return "SetToolTipText";
                if (span.SequenceEqual("SelectedValues".AsSpan()))
                    return "SelectedValues";
                if (span.SequenceEqual("SortExpression".AsSpan()))
                    return "SortExpression";
                break;
            case 'U':
                if (span.SequenceEqual("UpdateProgress".AsSpan()))
                    return "UpdateProgress";
                break;
            case 'W':
                if (span.SequenceEqual("WrapperTagName".AsSpan()))
                    return "WrapperTagName";
                break;
            case 'a':
                if (span.SequenceEqual("accept-charset".AsSpan()))
                    return "accept-charset";
                if (span.SequenceEqual("autocapitalize".AsSpan()))
                    return "autocapitalize";
                break;
            case 'c':
                if (span.SequenceEqual("controlCommand".AsSpan()))
                    return "controlCommand";
                break;
            case 'f':
                if (span.SequenceEqual("formnovalidate".AsSpan()))
                    return "formnovalidate";
                break;
            case 'k':
                if (span.SequenceEqual("ko.contextFor(".AsSpan()))
                    return "ko.contextFor(";
                break;
            case 'r':
                if (span.SequenceEqual("referrerpolicy".AsSpan()))
                    return "referrerpolicy";
                break;
        }
        break;
    case 15:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ContentTemplate".AsSpan()))
                    return "ContentTemplate";
                if (span.SequenceEqual("ControlProperty".AsSpan()))
                    return "ControlProperty";
                break;
            case 'D':
                if (span.SequenceEqual("DataContextType".AsSpan()))
                    return "DataContextType";
                break;
            case 'E':
                if (span.SequenceEqual("EnvironmentView".AsSpan()))
                    return "EnvironmentView";
                break;
            case 'F':
                if (span.SequenceEqual("FilterPlacement".AsSpan()))
                    return "FilterPlacement";
                break;
            case 'I':
                if (span.SequenceEqual("Items()?.length".AsSpan()))
                    return "Items()?.length";
                if (span.SequenceEqual("ItemTextBinding".AsSpan()))
                    return "ItemTextBinding";
                if (span.SequenceEqual("InvalidCssClass".AsSpan()))
                    return "InvalidCssClass";
                break;
            case 'O':
                if (span.SequenceEqual("OriginalMessage".AsSpan()))
                    return "OriginalMessage";
                break;
            case 'P':
                if (span.SequenceEqual("PrefixRouteName".AsSpan()))
                    return "PrefixRouteName";
                if (span.SequenceEqual("PostBackHandler".AsSpan()))
                    return "PostBackHandler";
                break;
            case 'U':
                if (span.SequenceEqual("UploadCompleted".AsSpan()))
                    return "UploadCompleted";
                break;
            case 'c':
                if (span.SequenceEqual("contenteditable".AsSpan()))
                    return "contenteditable";
                break;
        }
        break;
    case 16:
        switch (ch)
        {
            case '.':
                if (span.SequenceEqual(".Items()?.length".AsSpan()))
                    return ".Items()?.length";
                break;
            case 'A':
                if (span.SequenceEqual("AllowedFileTypes".AsSpan()))
                    return "AllowedFileTypes";
                break;
            case 'C':
                if (span.SequenceEqual("ConcurrencyQueue".AsSpan()))
                    return "ConcurrencyQueue";
                if (span.SequenceEqual("ClientIDFragment".AsSpan()))
                    return "ClientIDFragment";
                break;
            case 'D':
                if (span.SequenceEqual("DefaultRouteName".AsSpan()))
                    return "DefaultRouteName";
                break;
            case 'G':
                if (span.SequenceEqual("GoToDetailAction".AsSpan()))
                    return "GoToDetailAction";
                break;
            case 'H':
                if (span.SequenceEqual("HasClaimTemplate".AsSpan()))
                    return "HasClaimTemplate";
                if (span.SequenceEqual("HierarchyControl".AsSpan()))
                    return "HierarchyControl";
                break;
            case 'I':
                if (span.SequenceEqual("ItemValueBinding".AsSpan()))
                    return "ItemValueBinding";
                if (span.SequenceEqual("Inner-li:Visible".AsSpan()))
                    return "Inner-li:Visible";
                if (span.SequenceEqual("IsMemberTemplate".AsSpan()))
                    return "IsMemberTemplate";
                if (span.SequenceEqual("ItemTitleBinding".AsSpan()))
                    return "ItemTitleBinding";
                break;
            case 'L':
                if (span.SequenceEqual("LastPageTemplate".AsSpan()))
                    return "LastPageTemplate";
                break;
            case 'M':
                if (span.SequenceEqual("MarkupLineNumber".AsSpan()))
                    return "MarkupLineNumber";
                break;
            case 'N':
                if (span.SequenceEqual("NextPageTemplate".AsSpan()))
                    return "NextPageTemplate";
                break;
            case 'R':
                if (span.SequenceEqual("RenderWrapperTag".AsSpan()))
                    return "RenderWrapperTag";
                if (span.SequenceEqual("RequiredResource".AsSpan()))
                    return "RequiredResource";
                break;
            case 'S':
                if (span.SequenceEqual("SelectAllOnFocus".AsSpan()))
                    return "SelectAllOnFocus";
                if (span.SequenceEqual("SanitizedMessage".AsSpan()))
                    return "SanitizedMessage";
                if (span.SequenceEqual("SelectionChanged".AsSpan()))
                    return "SelectionChanged";
                break;
            case 'U':
                if (span.SequenceEqual("UploadButtonText".AsSpan()))
                    return "UploadButtonText";
                break;
            case 'd':
                if (span.SequenceEqual("dotvvm.postBack(".AsSpan()))
                    return "dotvvm.postBack(";
                break;
        }
        break;
    case 17:
        switch (ch)
        {
            case 'A':
                if (span.SequenceEqual("AuthenticatedView".AsSpan()))
                    return "AuthenticatedView";
                break;
            case 'C':
                if (span.SequenceEqual("ControlWithButton".AsSpan()))
                    return "ControlWithButton";
                break;
            case 'E':
                if (span.SequenceEqual("EditRowDecorators".AsSpan()))
                    return "EditRowDecorators";
                if (span.SequenceEqual("EmptyDataTemplate".AsSpan()))
                    return "EmptyDataTemplate";
                break;
            case 'F':
                if (span.SequenceEqual("FirstPageTemplate".AsSpan()))
                    return "FirstPageTemplate";
                if (span.SequenceEqual("FileUploadWrapper".AsSpan()))
                    return "FileUploadWrapper";
                break;
            case 'I':
                if (span.SequenceEqual("IsNamingContainer".AsSpan()))
                    return "IsNamingContainer";
                break;
            case 'P':
                if (span.SequenceEqual("PostBack.Handlers".AsSpan()))
                    return "PostBack.Handlers";
                break;
            case 'R':
                if (span.SequenceEqual("RequiredResources".AsSpan()))
                    return "RequiredResources";
                if (span.SequenceEqual("RenderSpanElement".AsSpan()))
                    return "RenderSpanElement";
                if (span.SequenceEqual("RefreshTextButton".AsSpan()))
                    return "RefreshTextButton";
                break;
            case 'S':
                if (span.SequenceEqual("SeparatorTemplate".AsSpan()))
                    return "SeparatorTemplate";
                break;
            case 'T':
                if (span.SequenceEqual("TextEditorControl".AsSpan()))
                    return "TextEditorControl";
                break;
            case 'U':
                if (span.SequenceEqual("UpdateTextOnInput".AsSpan()))
                    return "UpdateTextOnInput";
                break;
            case 'V':
                if (span.SequenceEqual("ValidationSummary".AsSpan()))
                    return "ValidationSummary";
                break;
        }
        break;
    case 18:
        switch (ch)
        {
            case 'A':
                if (span.SequenceEqual("AllowMultipleFiles".AsSpan()))
                    return "AllowMultipleFiles";
                break;
            case 'C':
                if (span.SequenceEqual("ControlWithButton2".AsSpan()))
                    return "ControlWithButton2";
                break;
            case 'E':
                if (span.SequenceEqual("EditCellDecorators".AsSpan()))
                    return "EditCellDecorators";
                break;
            case 'G':
                if (span.SequenceEqual("GridViewTextColumn".AsSpan()))
                    return "GridViewTextColumn";
                break;
            case 'H':
                if (span.SequenceEqual("HtmlGenericControl".AsSpan()))
                    return "HtmlGenericControl";
                break;
            case 'P':
                if (span.SequenceEqual("ParametrizedButton".AsSpan()))
                    return "ParametrizedButton";
                break;
            case 'S':
                if (span.SequenceEqual("SuccessMessageText".AsSpan()))
                    return "SuccessMessageText";
                break;
            case 'U':
                if (span.SequenceEqual("UsedPropertiesInfo".AsSpan()))
                    return "UsedPropertiesInfo";
                break;
            case 'V':
                if (span.SequenceEqual("ValidatorPlacement".AsSpan()))
                    return "ValidatorPlacement";
                break;
        }
        break;
    case 19:
        switch (ch)
        {
            case '(':
                if (span.SequenceEqual("(async ()=>{let vm=".AsSpan()))
                    return "(async ()=>{let vm=";
                break;
            case 'C':
                if (span.SequenceEqual("CurrentIndexBinding".AsSpan()))
                    return "CurrentIndexBinding";
                break;
            case 'H':
                if (span.SequenceEqual("HideWhenOnlyOnePage".AsSpan()))
                    return "HideWhenOnlyOnePage";
                if (span.SequenceEqual("HeaderRowDecorators".AsSpan()))
                    return "HeaderRowDecorators";
                if (span.SequenceEqual("HasNotClaimTemplate".AsSpan()))
                    return "HasNotClaimTemplate";
                break;
            case 'I':
                if (span.SequenceEqual("IsNotMemberTemplate".AsSpan()))
                    return "IsNotMemberTemplate";
                break;
            case 'S':
                if (span.SequenceEqual("ServerRenderedLabel".AsSpan()))
                    return "ServerRenderedLabel";
                break;
        }
        break;
    case 20:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ContentPlaceHolderID".AsSpan()))
                    return "ContentPlaceHolderID";
                if (span.SequenceEqual("CheckableControlBase".AsSpan()))
                    return "CheckableControlBase";
                break;
            case 'D':
                if (span.SequenceEqual("DotvvmBindableObject".AsSpan()))
                    return "DotvvmBindableObject";
                break;
            case 'H':
                if (span.SequenceEqual("HeaderCellDecorators".AsSpan()))
                    return "HeaderCellDecorators";
                break;
            case 'P':
                if (span.SequenceEqual("PreviousPageTemplate".AsSpan()))
                    return "PreviousPageTemplate";
                break;
            case 'S':
                if (span.SequenceEqual("ShowErrorMessageText".AsSpan()))
                    return "ShowErrorMessageText";
                if (span.SequenceEqual("ShowHeaderWhenNoData".AsSpan()))
                    return "ShowHeaderWhenNoData";
                break;
            case 'T':
                if (span.SequenceEqual("TemplatedListControl".AsSpan()))
                    return "TemplatedListControl";
                break;
            case 'k':
                if (span.SequenceEqual("ko.pureComputed(()=>".AsSpan()))
                    return "ko.pureComputed(()=>";
                break;
        }
        break;
    case 21:
        switch (ch)
        {
            case 'A':
                if (span.SequenceEqual("AuthenticatedTemplate".AsSpan()))
                    return "AuthenticatedTemplate";
                break;
            case 'C':
                if (span.SequenceEqual("ControlCommandBinding".AsSpan()))
                    return "ControlCommandBinding";
                break;
            case 'H':
                if (span.SequenceEqual("HideForAnonymousUsers".AsSpan()))
                    return "HideForAnonymousUsers";
                break;
            case 'I':
                if (span.SequenceEqual("IsEnvironmentTemplate".AsSpan()))
                    return "IsEnvironmentTemplate";
                break;
            case 'R':
                if (span.SequenceEqual("RenderAsNamedTemplate".AsSpan()))
                    return "RenderAsNamedTemplate";
                if (span.SequenceEqual("RecursiveTextRepeater".AsSpan()))
                    return "RecursiveTextRepeater";
                break;
            case 'S':
                if (span.SequenceEqual("SpaContentPlaceHolder".AsSpan()))
                    return "SpaContentPlaceHolder";
                break;
        }
        break;
    case 22:
        switch (ch)
        {
            case '(':
                if (span.SequenceEqual("(i)=>ko.unwrap(i).Id()".AsSpan()))
                    return "(i)=>ko.unwrap(i).Id()";
                break;
            case 'C':
                if (span.SequenceEqual("CompositeControlSample".AsSpan()))
                    return "CompositeControlSample";
                if (span.SequenceEqual("ConfirmPostBackHandler".AsSpan()))
                    return "ConfirmPostBackHandler";
                break;
            case 'G':
                if (span.SequenceEqual("GridViewTemplateColumn".AsSpan()))
                    return "GridViewTemplateColumn";
                if (span.SequenceEqual("GridViewCheckBoxColumn".AsSpan()))
                    return "GridViewCheckBoxColumn";
                break;
            case 'I':
                if (span.SequenceEqual("IsControlBindingTarget".AsSpan()))
                    return "IsControlBindingTarget";
                break;
            case 'T':
                if (span.SequenceEqual("TemplatedMarkupControl".AsSpan()))
                    return "TemplatedMarkupControl";
                break;
            case 'U':
                if (span.SequenceEqual("UploadErrorMessageText".AsSpan()))
                    return "UploadErrorMessageText";
                break;
        }
        break;
    case 23:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ConfigurableHtmlControl".AsSpan()))
                    return "ConfigurableHtmlControl";
                if (span.SequenceEqual("ConcurrencyQueueSetting".AsSpan()))
                    return "ConcurrencyQueueSetting";
                if (span.SequenceEqual("ControlPropertyUpdating".AsSpan()))
                    return "ControlPropertyUpdating";
                break;
            case 'I':
                if (span.SequenceEqual("IncludeErrorsFromTarget".AsSpan()))
                    return "IncludeErrorsFromTarget";
                break;
            case 'R':
                if (span.SequenceEqual("ResourceRequiringButton".AsSpan()))
                    return "ResourceRequiringButton";
                break;
            case 'S':
                if (span.SequenceEqual("ServerSideStylesControl".AsSpan()))
                    return "ServerSideStylesControl";
                if (span.SequenceEqual("SuppressPostBackHandler".AsSpan()))
                    return "SuppressPostBackHandler";
                break;
            case 'T':
                if (span.SequenceEqual("TextOrContentCapability".AsSpan()))
                    return "TextOrContentCapability";
                break;
            case 'i':
                if (span.SequenceEqual("inner-li:HtmlCapability".AsSpan()))
                    return "inner-li:HtmlCapability";
                break;
        }
        break;
    case 24:
        switch (ch)
        {
            case '(':
                if (span.SequenceEqual("(i)=>ko.unwrap(i).Text()".AsSpan()))
                    return "(i)=>ko.unwrap(i).Text()";
                if (span.SequenceEqual("(i)=>ko.unwrap(i).Name()".AsSpan()))
                    return "(i)=>ko.unwrap(i).Name()";
                break;
            case 'C':
                if (span.SequenceEqual("ConcurrencyQueueSettings".AsSpan()))
                    return "ConcurrencyQueueSettings";
                break;
            case 'I':
                if (span.SequenceEqual("IsNotEnvironmentTemplate".AsSpan()))
                    return "IsNotEnvironmentTemplate";
                break;
            case 'N':
                if (span.SequenceEqual("NotAuthenticatedTemplate".AsSpan()))
                    return "NotAuthenticatedTemplate";
                break;
            case 'R':
                if (span.SequenceEqual("ReferencedViewModuleInfo".AsSpan()))
                    return "ReferencedViewModuleInfo";
                if (span.SequenceEqual("RenderLinkForCurrentPage".AsSpan()))
                    return "RenderLinkForCurrentPage";
                break;
            case 'S':
                if (span.SequenceEqual("StopwatchPostbackHandler".AsSpan()))
                    return "StopwatchPostbackHandler";
                break;
        }
        break;
    case 25:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ControlPropertyValidation".AsSpan()))
                    return "ControlPropertyValidation";
                break;
            case 'E':
                if (span.SequenceEqual("ErrorCountPostbackHandler".AsSpan()))
                    return "ErrorCountPostbackHandler";
                break;
            case 'I':
                if (span.SequenceEqual("IncludeErrorsFromChildren".AsSpan()))
                    return "IncludeErrorsFromChildren";
                break;
        }
        break;
    case 26:
        switch (ch)
        {
            case 'N':
                if (span.SequenceEqual("NumberOfFilesIndicatorText".AsSpan()))
                    return "NumberOfFilesIndicatorText";
                break;
            case 'U':
                if (span.SequenceEqual("UseHistoryApiSpaNavigation".AsSpan()))
                    return "UseHistoryApiSpaNavigation";
                break;
        }
        break;
    case 27:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("CheckedItemsRepeaterWrapper".AsSpan()))
                    return "CheckedItemsRepeaterWrapper";
                break;
            case 'S':
                if (span.SequenceEqual("SortAscendingHeaderCssClass".AsSpan()))
                    return "SortAscendingHeaderCssClass";
                break;
        }
        break;
    case 28:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("CompositeControlWithTemplate".AsSpan()))
                    return "CompositeControlWithTemplate";
                break;
            case 'S':
                if (span.SequenceEqual("SortDescendingHeaderCssClass".AsSpan()))
                    return "SortDescendingHeaderCssClass";
                break;
            case 'h':
                if (span.SequenceEqual("http://www.w3.org/1999/xhtml".AsSpan()))
                    return "http://www.w3.org/1999/xhtml";
                break;
        }
        break;
    case 31:
        switch (ch)
        {
            case 'I':
                if (span.SequenceEqual("IsMasterPageCompositionFinished".AsSpan()))
                    return "IsMasterPageCompositionFinished";
                break;
        }
        break;
    case 32:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("CompileTimeLifecycleRequirements".AsSpan()))
                    return "CompileTimeLifecycleRequirements";
                if (span.SequenceEqual("CompositeListControlWithTemplate".AsSpan()))
                    return "CompositeListControlWithTemplate";
                break;
            case 'M':
                if (span.SequenceEqual("MarkupControlRegistrationControl".AsSpan()))
                    return "MarkupControlRegistrationControl";
                break;
        }
        break;
    case 33:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ControlControlCommandInvokeAction".AsSpan()))
                    return "ControlControlCommandInvokeAction";
                break;
        }
        break;
    case 34:
        switch (ch)
        {
            case 'A':
                if (span.SequenceEqual("AttributeToStringConversionControl".AsSpan()))
                    return "AttributeToStringConversionControl";
                break;
        }
        break;
    case 36:
        switch (ch)
        {
            case 'S':
                if (span.SequenceEqual("StaticCommand_ValueAssignmentControl".AsSpan()))
                    return "StaticCommand_ValueAssignmentControl";
                break;
            case 'd':
                if (span.SequenceEqual("dotvvm.evaluator.wrapObservable(()=>".AsSpan()))
                    return "dotvvm.evaluator.wrapObservable(()=>";
                break;
        }
        break;
    case 37:
        switch (ch)
        {
            case 'L':
                if (span.SequenceEqual("LifecycleRequirementsAssigningVisitor".AsSpan()))
                    return "LifecycleRequirementsAssigningVisitor";
                break;
        }
        break;
    case 39:
        switch (ch)
        {
            case 'd':
                if (span.SequenceEqual("dotvvm.globalize.bindingNumberToString(".AsSpan()))
                    return "dotvvm.globalize.bindingNumberToString(";
                break;
        }
        break;
    case 41:
        switch (ch)
        {
            case 'C':
                if (span.SequenceEqual("ComboBoxDataSourceBoundToStaticCollection".AsSpan()))
                    return "ComboBoxDataSourceBoundToStaticCollection";
                break;
        }
        break;
}

            str ??= new string(span);
            if (trySystemIntern)
                return string.IsInterned(str) ?? str;
            return str;
       }
    }
}
