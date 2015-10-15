using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Compilation;
using System.Reflection;
using DotVVM.Framework.Exceptions;

namespace DotVVM.Framework.Runtime
{
    /// <summary>
    /// Provides control builder objects for markup files.
    /// </summary>
    public class DefaultControlBuilderFactory : IControlBuilderFactory
    {
        private DotvvmConfiguration configuration;
        private IMarkupFileLoader markupFileLoader;

        public Func<IViewCompiler> ViewCompilerFactory { get; private set; }

        private ConcurrentDictionary<MarkupFile, IControlBuilder> controlBuilders = new ConcurrentDictionary<MarkupFile, IControlBuilder>();


        public DefaultControlBuilderFactory(DotvvmConfiguration configuration)
        {
            for (int i = 0; i < compilationLocks.Length; i++)
            {
                compilationLocks[i] = new object();
            }

            this.configuration = configuration;

            ViewCompilerFactory = () => configuration.ServiceLocator.GetService<IViewCompiler>();
            markupFileLoader = configuration.ServiceLocator.GetService<IMarkupFileLoader>();

            if (configuration.CompiledViewsAssemblies != null)
                foreach (var assembly in configuration.CompiledViewsAssemblies)
                {
                    LoadCompiledViewsAssembly(assembly);
                }
        }


        /// <summary>
        /// Gets the control builder.
        /// </summary>
        public IControlBuilder GetControlBuilder(string virtualPath)
        {
            var markupFile = markupFileLoader.GetMarkup(configuration, virtualPath);
            return controlBuilders.GetOrAdd(markupFile, CreateControlBuilder);
        }

        object[] compilationLocks = new object[Environment.ProcessorCount * 2];

        /// <summary>
        /// Creates the control builder.
        /// </summary>
        private IControlBuilder CreateControlBuilder(MarkupFile file)
        {
            var lockId = (file.GetHashCode() & 0x7fffffff) % compilationLocks.Length;
            // do not compile the same view multiple times
            lock (compilationLocks[lockId])
            {
                if (controlBuilders.ContainsKey(file)) return controlBuilders[file];

                var namespaceName = GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc);
                var assemblyName = namespaceName;
                var className = GetClassFromFileName(file.FileName) + "ControlBuilder";
                try
                {
                    return ViewCompilerFactory().CompileView(file.ContentsReaderFactory(), file.FileName, assemblyName, namespaceName, className);
                }
                catch (DotvvmCompilationException ex)
                {
                    if (ex.FileName == null)
                        ex.FileName = file.FullPath;
                    else if (!Path.IsPathRooted(ex.FileName))
                        ex.FileName = Path.Combine(
                            file.FullPath.Remove(file.FullPath.Length - file.FileName.Length),
                            ex.FileName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the name of the class from the file name.
        /// </summary>
        public static string GetClassFromFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        public static string GetNamespaceFromFileName(string fileName, DateTime lastWriteDateTimeUtc)
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
            return "DotvvmGeneratedViews" + fileName + "_" + lastWriteDateTimeUtc.Ticks;
        }

        public void LoadCompiledViewsAssembly(string filePath)
        {
            if (File.Exists(filePath))
            {
                LoadCompiledViewsAssembly(Assembly.LoadFile(Path.GetFullPath(filePath)));
            }
        }

        public void LoadCompiledViewsAssembly(Assembly assembly)
        {
            var builders = assembly.GetTypes().Select(t => new
            {
                type = t,
                attribute = t.GetCustomAttribute<LoadControlBuilderAttribute>()
            }).Where(t => t.attribute != null);
            foreach (var builder in builders)
            {
                RegisterControlBuilder(builder.attribute.FilePath, (IControlBuilder)Activator.CreateInstance(builder.type));
            }
        }

        public void RegisterControlBuilder(string file, IControlBuilder builder)
        {
            controlBuilders.TryAdd(markupFileLoader.GetMarkup(configuration, file), builder);
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