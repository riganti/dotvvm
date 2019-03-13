using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost.Operations
{
    public interface IOperation
    {
        string OperationName { get; }
        OperationResult Execute(IResult result, IOutputLogger logger);
    }
}