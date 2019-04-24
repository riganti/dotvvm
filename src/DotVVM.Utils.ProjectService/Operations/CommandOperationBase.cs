using System;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations
{
    public abstract class CommandOperationBase : CommandRunner, IOperation
    {
        public abstract string OperationName { get; set; }
        public CsprojVersion? SupportedCsprojVersion { get; set; }
        protected CommandOperationBase()
        {
        }
        protected CommandOperationBase(CsprojVersion supportedCsprojVersion)
        {
            SupportedCsprojVersion = supportedCsprojVersion;
        }
        public abstract OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger);
        protected void VerifyCsprojVersion(IResolvedProjectMetadata metadata)
        {
            if (!SupportedCsprojVersion.HasValue) return;
            if (metadata.CsprojVersion != SupportedCsprojVersion)
            {
                throw new ArgumentException($"{Enum.GetName(typeof(CsprojVersion), metadata.CsprojVersion)} csproj is not supported in current operation.");
            }
        }
    }
}