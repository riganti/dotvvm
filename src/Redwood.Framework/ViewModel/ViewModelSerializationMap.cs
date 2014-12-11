using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelSerializationMap
    {
        public Type Type { get; set; }
        public IEnumerable<ViewModelPropertyMap> Properties = new List<ViewModelPropertyMap>();

        Action<JObject, JsonSerializer, object> readerFn;
        public Action<JObject, JsonSerializer, object> Reader
        {
            get { return readerFn ?? (readerFn = CreateReader()); }
        }

        Action<JsonWriter, object, JsonSerializer> writerFn;
        public Action<JsonWriter, object, JsonSerializer> Writer
        {
            get { return writerFn ?? (writerFn = CreateWriter()); }
        }

        Func<object> constructFn;
        public Func<object> Construct
        {
            get { return constructFn ?? (constructFn = CreateConstructor()); }
        }

        public Func<object> CreateConstructor()
        {
            var ex = Expression.Lambda<Func<object>>(Expression.New(Type));
            return ex.Compile();
        }

        public Action<JObject, JsonSerializer, object> CreateReader()
        {
            var block = new List<Expression>();
            var returnTarget = Expression.Label();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var value = Expression.Variable(Type, "value");
            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            block.AddRange(Properties.Select(p =>
            {
                // jobj["{p.Name}"].CreateReader()
                var propReader =
                    Expression.Call(
                        Expression.Property(jobj,
                            typeof(JObject).GetProperty("Item", typeof(JObject), new[] { typeof(string) }), Expression.Constant(p.Name)),
                        "CreateReader", Type.EmptyTypes);

                Expression callDeserialize;
                if (p.Crypto == CryptoSettings.AuthenticatedEncrypt)
                    callDeserialize = Expression.Call(typeof(CryptoSerializer).GetMethod("DecryptDeserialize"), Expression.Call(propReader, "ReadAsString", Type.EmptyTypes), Expression.Constant(p.Type));

                else callDeserialize = Expression.Call(serializer, typeof(JsonSerializer).GetMethod("Deserialize", new[] { typeof(JsonReader), typeof(Type) }), propReader, Expression.Constant(p.Type));



                // value.{p.Name} = 
                return Expression.Call(
                    value,
                    Type.GetProperty(p.Name).SetMethod,
                    // serializer.Deserialize();
                    Expression.Convert(callDeserialize, p.Type)
                );
            }));

            // return value;
            //block.Add(Expression.Return(returnTarget, Expression.Convert(value, typeof(object))));
            //block.Add(Expression.Label(returnTarget));
            block.Add(value);

            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object>>(Expression.Convert(Expression.Block(Type, new[] { value }, block), typeof(object)), jobj, serializer, valueParam);
            return ex.Compile();
        }

        public Action<JsonWriter, object, JsonSerializer> CreateWriter()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var value = Expression.Variable(Type, "value");
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));


            foreach (var p in Properties.Where(map => map.TransferToClient))
            {
                // writer.WritePropertyName("{p.Name"});
                block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(p.Name)));

                var prop = Expression.Convert(Expression.Property(value, p.Name), typeof(object));

                if (p.Crypto == CryptoSettings.AuthenticatedEncrypt)
                    // writer.WriteValue(CryptoSerializer.EncryptSerialize(value.{p.Name}));
                    block.Add(Expression.Call(writer, typeof(JsonWriter).GetMethod("WriteValue", new[] { typeof(string) }), Expression.Call(typeof(CryptoSerializer).GetMethod("EncryptSerialize"), prop)));
                else
                    // serializer.Serialize(writer, value.{p.Name});
                    block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, prop));
            }

            block.Add(Expression.Call(writer, "WriteEndObject", Type.EmptyTypes));


            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer>>(Expression.Block(new[] { value }, block), writer, valueParam, serializer);

            return ex.Compile();
        }

    }
}
