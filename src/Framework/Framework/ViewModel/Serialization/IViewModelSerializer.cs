using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelSerializer
    {
        void BuildViewModel(IDotvvmRequestContext context, object? commandResult);

        string BuildStaticCommandResponse(IDotvvmRequestContext context, object? commandResult, string[]? knownTypeMetadata = null);

        string SerializeViewModel(IDotvvmRequestContext context);

        string SerializeModelState(IDotvvmRequestContext context);

        string SerializeErrorResponse(string action, string errorMessage);

        void PopulateViewModel(IDotvvmRequestContext context, string serializedPostData);

        Task<StaticCommandRequest> DeserializeStaticCommandRequest(IDotvvmRequestContext context);

        ActionInfo? ResolveCommand(IDotvvmRequestContext context, DotvvmView view);

        void AddPostBackUpdatedControls(IDotvvmRequestContext context, IEnumerable<(string name, string html)> postbackUpdatedControls);

        void AddNewResources(IDotvvmRequestContext context);
    }
}
