namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public interface IOperationProvider
    {
        IOperation GetOperation(IResult result);
    }
}