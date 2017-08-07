using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Diagnostics
{

    public interface IDiagnosticsInformationSender
    {
        Task SendInformationAsync(DiagnosticsInformation information);
    }

}