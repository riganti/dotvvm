using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Provides control builder objects for markup files.
    /// </summary>
    public class DefaultControlBuilderFactory : IControlBuilderFactory
    {
        public Func<IViewCompiler> ViewCompilerFactory { get; set; }


        // TODO: this cache may cause problems when multiple incompatible compilers are used on the same view
        private static ConcurrentDictionary<MarkupFile, IControlBuilder> controlBuilders = new ConcurrentDictionary<MarkupFile, IControlBuilder>();



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
            var namespaceName = GetNamespaceFromFileName(file.FileName);
            var assemblyName = namespaceName;
            var className = GetClassFromFileName(file.FileName) + "ControlBuilder";
            
            return ViewCompilerFactory().CompileView(file.ContentsReaderFactory(), file.FileName, assemblyName, namespaceName, className);
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
        internal static string GetNamespaceFromFileName(string fileName)
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