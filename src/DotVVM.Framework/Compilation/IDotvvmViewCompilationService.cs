using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
    public interface IDotvvmViewCompilationService
    {
        IEnumerable<DotHtmlFileInfo> FilesWithErrors { get; }

        List<DotHtmlFileInfo> GetMasterPages();
        List<DotHtmlFileInfo> GetControls();
        List<DotHtmlFileInfo> GetRoutes();

        Task<bool> CompileAll(bool buildInParallel, bool forceRecompile = false);
        bool BuildView(DotHtmlFileInfo file, ConcurrentBag<DotHtmlFileInfo> tempList);
    }
}