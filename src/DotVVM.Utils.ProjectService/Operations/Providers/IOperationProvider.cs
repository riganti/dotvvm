using System;

namespace DotVVM.Utils.ConfigurationHost.Operations.Providers
{
    public interface IOperationProvider
    {
        IOperation GetOperation(IResult result);
    }
}