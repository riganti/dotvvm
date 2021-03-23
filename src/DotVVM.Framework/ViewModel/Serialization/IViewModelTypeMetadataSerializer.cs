using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelTypeMetadataSerializer
    {
        JToken SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, ISet<string> knownTypeIds = null);
    }

}
