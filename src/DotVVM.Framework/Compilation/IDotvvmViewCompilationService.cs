using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
    public interface IDotvvmViewCompilationService
    {
        /// <summary>
        /// Gets all DotHtmlFileInfos with Status CompilationFailed from last compilation.
        /// </summary>
        IEnumerable<DotHtmlFileInfo> GetFilesWithFailedCompilation();

        /// <summary>
        /// Returns all currently known masterpages.
        /// </summary>
        List<DotHtmlFileInfo> GetMasterPages();

        /// <summary>
        /// Returns all discovered controls.
        /// </summary>
        List<DotHtmlFileInfo> GetControls();

        /// <summary>
        /// Returns all discovered routes.
        /// </summary>
        /// <returns></returns>
        List<DotHtmlFileInfo> GetRoutes();

        /// <summary>
        /// Compiles all routes,controls and masterpages which have been not compiled before.
        /// </summary>
        /// <param name="buildInParallel">Compilation would run in parallel if set to true.</param>
        /// <param name="forceRecompile">Everything will be recompiled if set to true.</param>
        /// <returns>False if any errors are found</returns>
        Task<bool> CompileAll(bool buildInParallel, bool forceRecompile = false);

        /// <summary>
        /// Builds given DotHtml file.
        /// </summary>
        /// <param name="file">File to compile</param>
        /// <param name="foundMasterpages">All found masterpages would be added to this collection during file compilation.</param>
        /// <returns></returns>
        bool BuildView(DotHtmlFileInfo file, ConcurrentBag<DotHtmlFileInfo> foundMasterpages = null);
    }
}
