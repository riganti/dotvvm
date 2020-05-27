using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotVVM.Diagnostics.StatusPage
{
    public interface IDotvvmViewCompilationService
    {
        IEnumerable<DotHtmlFileInfo> FilesWithErrors { get; }

        List<DotHtmlFileInfo> GetMasterPages();
        List<DotHtmlFileInfo> GetControls();
        List<DotHtmlFileInfo> GetRoutes();

        Task<bool> CompileAll(bool forceRecompile=false);
        bool BuildView(DotHtmlFileInfo file, ConcurrentBag<DotHtmlFileInfo> tempList);
    }
}