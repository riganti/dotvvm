using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Diagnostics
{

    public interface IDiagnosticsDataSender
    {
        Task SendDataAsync(DiagnosticsData data);
    }

}