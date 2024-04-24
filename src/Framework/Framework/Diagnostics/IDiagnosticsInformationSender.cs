using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Diagnostics
{

    public interface IDiagnosticsInformationSender
    {
        Task SendInformationAsync(DiagnosticsInformation information);
        DiagnosticsInformationSenderState State { get; }
    }

    public enum DiagnosticsInformationSenderState
    {
        /// <summary> No events are being sent, collection is unnecessary </summary>
        Disconnected,
        /// <summary> Only timing information is being collected, view models and other large objects are not neccessary to collect </summary>
        TimingOnly,
        /// <summary> All information is being collected </summary>
        Full
    }

}
