using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelServerCache
    {

        string StoreViewModel(IDotvvmRequestContext context, Stream data);

        JsonElement TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JsonElement viewModelDiffToken);

    }
}
