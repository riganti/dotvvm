using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
    public interface IDotvvmViewCompilationService
    {
        /// <summary>
        /// Gets all DotHtmlFileInfos with Status CompilationFailed from last compilation.
        /// </summary>
        ImmutableArray<DotHtmlFileInfo> GetFilesWithFailedCompilation();
        
        /// <summary>
        /// Returns all currently known masterpages.
        /// </summary>
        ImmutableArray<DotHtmlFileInfo> GetMasterPages();

        /// <summary>
        /// Returns all discovered controls.
        /// </summary>
        ImmutableArray<DotHtmlFileInfo> GetControls();

        /// <summary>
        /// Returns all discovered routes.
        /// </summary>
        /// <returns></returns>
        ImmutableArray<DotHtmlFileInfo> GetRoutes();
        
        /// <summary>
        /// Compiles all view which have not been compiled yet.
        /// </summary>
        /// <param name="buildInParallel">If set, than the compilations will be performed in parallel.</param>
        /// <param name="forceRecompile">If set, than everything will be recompiled.</param>
        /// <returns>Returns whether compilation was successful.</returns>
        Task<bool> CompileAll(bool buildInParallel=true, bool forceRecompile = false);
    }
}
