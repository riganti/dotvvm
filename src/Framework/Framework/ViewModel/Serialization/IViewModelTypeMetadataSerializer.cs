using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelTypeMetadataSerializer
    {
        void SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, Utf8JsonWriter json, ReadOnlySpan<byte> propertyName, ISet<string>? knownTypeIds = null);
    }

}
