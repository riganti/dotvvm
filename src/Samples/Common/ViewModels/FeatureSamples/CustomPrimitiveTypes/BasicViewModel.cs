using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class BasicViewModel : DotvvmViewModelBase
    {

        [FromRoute("id")]
        public SampleId IdInRoute { get; set; }

        [FromQuery("id")]
        public SampleId? IdInQuery { get; set; }

        [Required]
        public SampleId SelectedItemId { get; set; }

        [Required]
        public SampleId? SelectedItemNullableId { get; set; }


        // použít typové ID ve viewmodelu
        // [Required] validace
        // musí fungovat i když je nullable

        // vybírání hodnot v comboboxu, checkboxu + opět s podporou nullable


        // routing - <dot:RouteLink Param-Id={value: TypeId} />

        // [FromRoute] a [FromQuery]

        // JS translations

        public List<SampleItem> Items { get; set; } = new List<SampleItem>
        {
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("96c37b99-5fd5-448c-8a64-977ae11b8b8b")), Text = "Item 1" },
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("c2654a1f-3781-49a8-911b-c7346db166e0")), Text = "Item 2" },
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("e467a201-9ab7-4cd5-adbf-66edd03f6ae1")), Text = "Item 3" },
        };

    }

    public class SampleItem
    {
        public SampleId Id { get; set; }
        public string Text { get; set; }
    }


    [JsonConverter(typeof(TypeIdJsonConverter<SampleId>))]
    public record SampleId : TypeId<SampleId>
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }
    }

    public abstract record TypeId<TId> : ITypeId
        where TId : TypeId<TId>
    {
        public Guid IdValue { get; }

        protected TypeId(Guid idValue)
        {
            if (idValue == default) throw new ArgumentException(nameof(idValue));
            IdValue = idValue;
        }

        public static TId CreateNew()
        {
            var guid = Guid.NewGuid();
            return (TId)Activator.CreateInstance(typeof(TId), args: guid)!;
        }

        public static TId CreateExisting(Guid idValue)
        {
            if (idValue == default) throw new ArgumentException(nameof(idValue));
            return (TId)Activator.CreateInstance(typeof(TId), args: idValue)!;
        }

        public override string ToString()
        {
            return $"{GetType()} {{{IdValue}}}";
        }
    }

    public interface ITypeId
    {
        Guid IdValue { get; }
    }

    public class TypeIdJsonConverter<TId> : JsonConverter where TId : TypeId<TId>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TId); 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var idText = reader.ReadAsString();
                var idValue = Guid.Parse(idText);
                return TypeId<TId>.CreateExisting(idValue);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw new JsonSerializationException($"Token {reader.TokenType} cannot be deserialized as TypeId!");
            }
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((value as ITypeId)?.IdValue);
        }

    }

}

