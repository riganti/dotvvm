using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelServerCache
    {

        string StoreViewModel(IDotvvmRequestContext context, JObject viewModelToken);

        JObject TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JObject viewModelDiffToken);

    }
}
