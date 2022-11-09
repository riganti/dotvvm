using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelTypeMetadataSerializer
    {
        JObject SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, ISet<string>? knownTypeIds = null);
    }

}
