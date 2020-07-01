using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.Compilation
{
    public class NamingHelper
    {
        /// <summary>
        /// Gets the name of the class from the file name.
        /// </summary>
        public static string GetClassFromFileName(string fileName)
        {
            return GetValidIdentifier(Path.GetFileNameWithoutExtension(fileName));
        }

        public static string GetValidIdentifier(string identifier)
        {
            if (String.IsNullOrEmpty(identifier)) return "_";
            var arr = identifier.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (!Char.IsLetterOrDigit(arr[i]))
                {
                    arr[i] = '_';
                }
            }
            identifier = new string(arr);
            if (Char.IsDigit(arr[0])) identifier = "C" + identifier;
            if (csharpKeywords.Contains(identifier)) identifier += "0";
            return identifier;
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        public static string GetNamespaceFromFileName(string fileName, DateTime lastWriteDateTimeUtc, string namePrefix)
        {
            // TODO: make sure crazy directory names are ok, it should also work on linux :)

            // replace \ and / for .
            var parts = fileName.Split(new[] { '/', '\\' });
            parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

            fileName = String.Join(".", parts.Select(GetValidIdentifier));
            return namePrefix + fileName + "_" + lastWriteDateTimeUtc.Ticks;
        }

        private static readonly HashSet<string> csharpKeywords = new HashSet<string>(new[]
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "descending", "dynamic", "from", "get", "global", "group", "into",
            "join", "let", "orderby", "partial", "remove", "select", "set", "value", "var", "where", "where", "yield"
        });
    }
}
