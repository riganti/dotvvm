using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Diagnostics
{
    public static class DotvvmCompilationStatusApi
    {
        public static async Task<(int StatusCode, string? ResponseBody)> GetStatusResponse(IDotvvmViewCompilationService compilationService)
        {
            var result = await compilationService.CompileAll(buildInParallel: true);
            if (result)
            {
                return (200, null);
            }

            return (
                500,
                JsonSerializer.Serialize(
                    compilationService.GetFilesWithFailedCompilation(),
                    DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe
                )
            );
        }
    }
}
