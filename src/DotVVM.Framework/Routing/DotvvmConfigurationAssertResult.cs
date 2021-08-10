#nullable enable
namespace DotVVM.Framework.Routing
{
    internal class DotvvmConfigurationAssertResult<T>
    {

        public DotvvmConfigurationAssertResult(T route, DotvvmConfigurationAssertReason missingFile)
        {
            this.Value = route;
            this.Reason = missingFile;
        }

        public T Value { get; set; }
        public DotvvmConfigurationAssertReason Reason { get; set; }
    }


}
