using System;
using System.Collections.Generic;
using System.Text.Json;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelSerializer
    {
        JsonSerializerOptions ViewModelJsonOptions { get; }
        ReadOnlyMemory<byte> BuildStaticCommandResponse(IDotvvmRequestContext context, object? commandResult, string[]? knownTypeMetadata = null);

        string SerializeViewModel(IDotvvmRequestContext context, object? commandResult = null, IEnumerable<(string name, string html)>? postbackUpdatedControls = null, bool serializeNewResources = false);

        byte[] SerializeModelState(IDotvvmRequestContext context);

        void PopulateViewModel(IDotvvmRequestContext context, ReadOnlyMemory<byte> serializedPostData);

        ActionInfo ResolveCommand(IDotvvmRequestContext context, DotvvmView view);
    }
}
