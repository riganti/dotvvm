using DotVVM.Framework.ViewModel;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ReturnedFile
{
    public class ReturnedFileSampleViewModel: DotvvmViewModelBase
    {
        public string Text { get; set; } = "";
        public int IncludedCommandCount { get; set; }
        public int IncludedStaticCommandCount { get; set; }

        public override async Task Init()
        {
            if (Context.Query["includeOnInit"] == "1")
            {
                await Context.IncludeReturnedFileAsync(Encoding.UTF8.GetBytes("included init"), "included-init.txt", "text/plain");
            }

            await base.Init();
        }

        public void GetFile()
        {
            IncludedCommandCount++; // no action, postback is aborted
            Context.ReturnFile(Encoding.UTF8.GetBytes(Text), "file.txt", "text/plain");
        }
        public void GetFileInline()
        {
            IncludedCommandCount++; // no action, postback is aborted
            Context.ReturnFile(Encoding.UTF8.GetBytes(Text), "file.txt", "text/plain", attachmentDispositionType: "inline");
        }
        public async Task IncludeFile()
        {
            IncludedCommandCount++;
            await Context.IncludeReturnedFileAsync(Encoding.UTF8.GetBytes(Text), "included-file.txt", "text/plain");
        }
        public async Task IncludeFileInline()
        {
            IncludedCommandCount++;
            await Context.IncludeReturnedFileAsync(Encoding.UTF8.GetBytes(Text), "included-inline-file.txt", "text/plain", attachmentDispositionType: "inline");
        }

        [AllowStaticCommand]
        public static async Task<int> IncludeFileStatic(IDotvvmRequestContext context, string text, int count)
        {
            await context.IncludeReturnedFileAsync(Encoding.UTF8.GetBytes(text), "included-static-file.txt", "text/plain");
            return count + 1;
        }
    }
}
