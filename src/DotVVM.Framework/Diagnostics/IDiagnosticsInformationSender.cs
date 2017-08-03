using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Diagnostics
{

    public interface IDiagnosticsInformationSender
    {
        Task SendDataAsync(DiagnosticsInformation information);
    }

}