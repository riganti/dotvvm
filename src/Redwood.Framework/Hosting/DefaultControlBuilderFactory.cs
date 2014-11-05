using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Controls;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Provides control builder objects for markup files.
    /// </summary>
    public class DefaultControlBuilderFactory : IControlBuilderFactory
    {
        private readonly IViewCompiler compiler;
        private ConcurrentDictionary<MarkupFile, Func<RedwoodControl>> controlBuilders;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControlBuilderFactory"/> class.
        /// </summary>
        public DefaultControlBuilderFactory(IViewCompiler compiler)
        {
            this.compiler = compiler;
            this.controlBuilders = new ConcurrentDictionary<MarkupFile, Func<RedwoodControl>>();
        }

        /// <summary>
        /// Gets the control builder.
        /// </summary>
        public Func<RedwoodControl> GetControlBuilder(MarkupFile markupFile)
        {
            return controlBuilders.GetOrAdd(markupFile, CreateControlBuilder);
        }

        /// <summary>
        /// Creates the control builder.
        /// </summary>
        private Func<RedwoodControl> CreateControlBuilder(MarkupFile file)
        {
            var namespaceName = GetNamespaceFromFileName(file.FileName);
            var assemblyName = namespaceName + GetClassFromFileName(file) + ".dll";
            var className = GetClassFromFileName(file) + "ControlBuilder";

            return compiler.CompileView(file.ContentsReader, file.FileName, assemblyName, namespaceName, className);
        }

        /// <summary>
        /// Gets the name of the class from the file name.
        /// </summary>
        private static string GetClassFromFileName(MarkupFile file)
        {
            return Path.GetFileNameWithoutExtension(file.FileName);
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        private static string GetNamespaceFromFileName(string fileName)
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

            // get rid of the file name
            fileName = fileName.Substring(fileName.LastIndexOf('.') + 1).Trim('.');
            if (fileName != string.Empty)
            {
                fileName = "." + fileName;
            }
            return "RedwoodGeneratedViews" + fileName;
        }
    }
}