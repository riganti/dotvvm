using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations
{
    public interface IOperation
    {
        string OperationName { get; }
        OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger);
    }
}