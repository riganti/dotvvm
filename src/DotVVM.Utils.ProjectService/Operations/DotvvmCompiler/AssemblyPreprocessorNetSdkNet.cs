namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class AssemblyPreprocessorNetSdkNet : AssemblyPreprocessorBase
    {
        public AssemblyPreprocessorNetSdkNet(IResolvedProjectMetadata metadata, string compilerPath) : base(metadata, compilerPath)
        {
        }

        public override void CreateBindings()
        {
            //TODO: Process deps json of web assembly into app.config of compiler 
        }
    }
}