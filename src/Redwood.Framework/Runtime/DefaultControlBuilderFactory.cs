using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Configuration;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime.Compilation;

namespace Redwood.Framework.Runtime
{
    /// <summary>
    /// Provides control builder objects for markup files.
    /// </summary>
    public class DefaultControlBuilderFactory : IControlBuilderFactory
    {
        public Lazy<IViewCompiler> ViewCompiler { get; private set; }


        private ConcurrentDictionary<MarkupFile, IControlBuilder> controlBuilders = new ConcurrentDictionary<MarkupFile, IControlBuilder>();


        public DefaultControlBuilderFactory(RedwoodConfiguration configuration)
        {
            ViewCompiler = new Lazy<IViewCompiler>(() => configuration.ServiceLocator.GetService<IViewCompiler>());
        }


        /// <summary>
        /// Gets the control builder.
        /// </summary>
        public IControlBuilder GetControlBuilder(MarkupFile markupFile)
        {
            return controlBuilders.GetOrAdd(markupFile, CreateControlBuilder);
        }

        /// <summary>
        /// Creates the control builder.
        /// </summary>
        private IControlBuilder CreateControlBuilder(MarkupFile file)
        {
            var namespaceName = GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc);
            var assemblyName = namespaceName;
            var className = GetClassFromFileName(file.FileName) + "ControlBuilder";
            
            return ViewCompiler.Value.CompileView(file.ContentsReaderFactory(), file.FileName, assemblyName, namespaceName, className);
        }

        /// <summary>
        /// Gets the name of the class from the file name.
        /// </summary>
        internal static string GetClassFromFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        internal static string GetNamespaceFromFileName(string fileName, DateTime lastWriteDateTimeUtc)
        {
            // remove extension
            fileName = fileName.Substring(0, fileName.Length - MarkupFile.ViewFileExtension.Length);

            // replace \ and / for .
            fileName = fileName.Replace('/', '.').Replace('\\', '.');

            // remove spaces
            foreach (var ch in Path.GetInvalidPathChars())
            {
                fileName = fileName.Replace(ch, '_');
            }

            // get rid of the extension
            fileName = fileName.Substring(fileName.LastIndexOf('.') + 1).Trim('.');
            if (fileName != string.Empty)
            {
                fileName = "." + fileName;
            }

            // make sure any part of the filename is not a C# keyword
            var parts = fileName.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (csharpKeywords.Contains(parts[i]))
                {
                    parts[i] += "0";
                }
            }
            fileName = string.Join(".", parts);
            return "RedwoodGeneratedViews" + fileName + "_" + lastWriteDateTimeUtc.Ticks;
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