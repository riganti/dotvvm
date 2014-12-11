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
        Func<JObject, JsonSerializer, object> readerFn;
        public Func<JObject, JsonSerializer, object> Reader
        {
            get { return readerFn ?? (readerFn = CreateReader()); }
        }
        Action<JsonWriter, object, JsonSerializer> writerFn;
        public Action<JsonWriter, object, JsonSerializer> Writer
        {
            get { return writerFn ?? (writerFn = CreateWriter()); }
        }

        public Func<JObject, JsonSerializer, object> CreateReader()
        {
            var block = new List<Expression>();
            var returnTarget = Expression.Label();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var value = Expression.Variable(Type, "value");
            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.New(Type)));

            block.AddRange(Properties.Select(p =>
            {
                // jobj["{p.Name}"].CreateReader()
                var propReader =
                    Expression.Call(
                        Expression.Property(jobj,
                            typeof(JObject).GetProperty("Item", typeof(JObject), new[] { typeof(string) }), Expression.Constant(p.Name)),
                        "CreateReader", Type.EmptyTypes);

                var callDeserialize = Expression.Call(serializer, typeof(JsonSerializer).GetMethod("Deserialize", new [] { typeof(JsonReader), typeof(Type) }), propReader, Expression.Constant(p.Type));

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

            var ex = Expression.Lambda<Func<JObject, JsonSerializer, object>>(Expression.Convert(Expression.Block(Type, new[] { value }, block), typeof(object)), jobj, serializer);
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

                // serializer.Serialize(writer, value.{p.Name});
                block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, Expression.Property(value, p.Name)));
            }

            block.Add(Expression.Call(writer, "WriteEndObject", Type.EmptyTypes));


            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer>>(Expression.Block(new[] { value }, block), writer, valueParam, serializer);

            return ex.Compile();
        }


    }
}
