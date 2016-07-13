using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System.Runtime.Loader;
using Microsoft.DotNet.InternalAbstractions;

namespace DotVVM.Framework.Compilation
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
            return GetValidIdentifier(Path.GetFileNameWithoutExtension(fileName));
        }

        protected static string GetValidIdentifier(string identifier)
        {
            var arr = identifier.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (!char.IsLetterOrDigit(arr[i]))
                {
                    arr[i] = '_';
                }
            }
            identifier = new string(arr);
            if (char.IsDigit(arr[0])) identifier = "C" + identifier;
            if (csharpKeywords.Contains(identifier)) identifier += "0";
            return identifier;
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        public static string GetNamespaceFromFileName(string fileName, DateTime lastWriteDateTimeUtc)
        {
            // TODO: make sure crazy directory names are ok, it should also work on linux :)

            // replace \ and / for .
            var parts = fileName.Split(new[] { '/', '\\' });
            parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

            fileName = string.Join(".", parts.Select(GetValidIdentifier));
            return "DotvvmGeneratedViews" + fileName + "_" + lastWriteDateTimeUtc.Ticks;
        }

		public void LoadCompiledViewsAssembly(string filePath)
		{
			var assembly = TryFindAssembly(filePath);
			if (assembly != null)
			{
				LoadCompiledViewsAssembly(assembly);

				var bindings = Path.Combine(Path.GetDirectoryName(assembly.GetCodeBasePath()), "CompiledViewsBindings.dll");
				if (File.Exists(bindings)) AssemblyLoader.LoadFile(bindings);
			}
		}

		public Assembly TryFindAssembly(string fileName)
		{
			if (File.Exists(fileName)) return AssemblyLoader.LoadFile(fileName);
			if (Path.IsPathRooted(fileName)) return null;
			var cleanName = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
			string a;
			foreach (var assembly in ReflectionUtils.GetAllAssemblies())
			{
				// get already loaded assembly
				if (assembly.GetName().Name == cleanName)
				{
					var codeBase = assembly.GetCodeBasePath();
					if (codeBase.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) return assembly;
				}
			}
			foreach (var assembly in new[] { typeof(DefaultControlBuilderFactory).GetTypeInfo().Assembly.GetCodeBasePath(), configuration.ApplicationPhysicalPath })
			{
				if (!string.IsNullOrEmpty(assembly))
				{
					a = Path.Combine(Path.GetDirectoryName(assembly), fileName);
					if (File.Exists(a)) return AssemblyLoader.LoadFile(a);
				}
			}
			return null;
		}

		public void LoadCompiledViewsAssembly(Assembly assembly)
		{
			var builders = assembly.GetTypes().Select(t => new
			{
				type = t,
				attribute = t.GetTypeInfo().GetCustomAttribute<LoadControlBuilderAttribute>()
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