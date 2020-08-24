using System.IO;

namespace DotVVM.CommandLine
{
    public class ShimCreationContext
    {
        public ShimCreationContext(
            FileInfo project,
            DirectoryInfo dotvvmDirectory,
            FileInfo? app)
        {
            Project = project;
            DotvvmDirectory = dotvvmDirectory;
            App = app;
        }

        /// <summary>
        /// The DotVVM web project the shim is generated for.
        /// </summary>
        public FileInfo Project { get; }

        /// <summary>
        /// The '.dotvvm' directory inside the targetted <see cref="Project" />.
        /// </summary>
        public DirectoryInfo DotvvmDirectory { get; }

        /// <summary>
        /// The app that is being shimmed.
        /// </summary>
        public FileInfo? App { get; }
    }
}
