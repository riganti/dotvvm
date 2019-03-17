using System.IO;
using DotVVM.Utils.ProjectService.Extensions;
using Newtonsoft.Json.Linq;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class AssemblyPreprocessorNetSdkCore : AssemblyPreprocessorBase
    {
        public AssemblyPreprocessorNetSdkCore(IResult result, string compilerPath) : base(result, compilerPath)
        {
        }

        public override void CreateBindings()
        {
            //ProcessDepsJson();
        }

        private void ProcessDepsJson()
        {
            var inPath = Path.Combine(Path.GetDirectoryName(Result.GetWebsiteAssemblyPath()),
                Result.AssemblyName + ".deps.json");
            var outPath = Path.GetFileNameWithoutExtension(CompilerPath) + ".deps.json";
            File.Copy(inPath, outPath, true);
            string jsonString;
            using (StreamReader r = new StreamReader(outPath))
            {
                jsonString = r.ReadToEnd();
            }

            var json = JObject.Parse(jsonString);
            //TODO: Process json as needed
        }
    }
}
