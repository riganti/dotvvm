using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations
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
        public abstract OperationResult Execute(IResult result, IOutputLogger logger);
        protected void VerifyCsprojVersion(IResult result)
        {
            if (!SupportedCsprojVersion.HasValue) return;
            if (result.CsprojVersion != SupportedCsprojVersion)
            {
                throw new ArgumentException($"{Enum.GetName(typeof(CsprojVersion), result.CsprojVersion)} csproj is not supported in current operation.");
            }
        }
    }
}