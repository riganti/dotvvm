using System.Runtime.InteropServices;

namespace DotVVM.Utils.ConfigurationHost.Operations
{
    public class OperationResult
    {
        public string OperationName { get; set; }
        public bool Executed { get; set; }
        public bool Successful { get; set; }
    }
}