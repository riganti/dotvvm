using System;
using System.IO;
using Newtonsoft.Json.Linq;
using DotVVM.Utils.ConfigurationHost.Extensions;

namespace DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler
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