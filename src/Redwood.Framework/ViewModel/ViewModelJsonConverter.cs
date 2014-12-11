using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelJsonConverter : JsonConverter
    {
        /// <summary>
        /// Dictionary of view model serializers
        /// </summary>
        readonly static Dictionary<Type, ViewModelSerializationMap> Maps = ViewModelSerializationMapper.MapAllViewModels().ToDictionary(m => m.Type);

        public override bool CanConvert(Type objectType)
        {
            return Maps.ContainsKey(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var map = Maps[objectType];
            var instance = map.Construct();
            map.Reader(JObject.Load(reader), serializer, instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Maps[value.GetType()].Writer(writer, value, serializer);
        }

        public virtual void Populate(JObject jobj, JsonSerializer serializer, object value)
        {
            Maps[value.GetType()].Reader(jobj, serializer, value);
        }
    }
}
