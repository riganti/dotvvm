using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public class ViewModelSerializationMap
    {
        private const string CLIENT_EXTENDERS_KEY = "clientExtenders";

        /// <summary>
        /// Gets or sets the object type for this serialization map.
        /// </summary>
        public Type Type { get; private set; }

        public IEnumerable<ViewModelPropertyMap> Properties { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationMap"/> class.
        /// </summary>
        public ViewModelSerializationMap(Type type, IEnumerable<ViewModelPropertyMap> properties)
        {
            Type = type;
            Properties = properties.ToList();
        }

        public void ResetFunctions()
        {
            readerFactory = null;
            writerFactory = null;
        }

        private Action<JObject, JsonSerializer, object, EncryptedValuesReader> readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, EncryptedValuesReader> ReaderFactory
        {
            get { return readerFactory ?? (readerFactory = CreateReaderFactory()); }
        }

        private Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> WriterFactory
        {
            get { return writerFactory ?? (writerFactory = CreateWriterFactory()); }
        }

        private Func<object> constructorFactory;
        /// <summary>
        /// Gets the constructor factory.
        /// </summary>
        public Func<object> ConstructorFactory
        {
            get { return constructorFactory ?? (constructorFactory = CreateConstructorFactory()); }
        }

        /// <summary>
        /// Creates the constructor for this object.
        /// </summary>
        public Func<object> CreateConstructorFactory()
        {
            var ex = Expression.Lambda<Func<object>>(Expression.New(Type));
            return ex.Compile();
        }

        /// <summary>
        /// Creates the reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, EncryptedValuesReader> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var encryptedValuesReader = Expression.Parameter(typeof(EncryptedValuesReader), "encryptedValuesReader");
            var value = Expression.Variable(Type, "value");

            // add current object to encrypted values, this is needed because one property can potentionaly contain more objects (is a collection)
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.IncrementIndex), Type.EmptyTypes));
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));

            // curly brackets are used for variables and methods from the scope of this factory method
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties)
            {
                block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.IncrementIndex), Type.EmptyTypes));
                if(!property.TransferToServer || property.PropertyInfo.SetMethod == null)
                {
                    continue;
                }

                if (property.ViewModelProtection == ProtectMode.EncryptData || property.ViewModelProtection == ProtectMode.SignData)
                {
                    // value.{PropertyInfo} = ({PropertyInfo.PropertyType})Deserialize(serializer, encryptedValuesReader.ReadValue(), {property}, (object)value.{PropertyInfo});
                    block.Add(Expression.Call(
                        value,
                        property.PropertyInfo.SetMethod,
                        Expression.Convert(
                            ExpressionUtils.Replace(
                                (JsonSerializer s, EncryptedValuesReader ev, object existing) => Deserialize(s, ev.ReadValue(), property, existing),
                                serializer, encryptedValuesReader,
                                    Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object))),
                            property.Type)
                        ));
                }
                else
                {
                    var checkEV = CanContainEncryptedValues(property.Type);
                    if (checkEV)
                    {
                        // encryptedValuesReader.Nest();
                        block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));
                    }

                    // jobj[{property.Name}]
                    var jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);

                    // object helperValueThatsNotActuallyThere = {property.Populate} ? (object)value.{property} : null;
                    // if (jobj[{property.Name}] != null)
                    // {
                    //     value.{property.Name} = ({property.Type})Deserialize(serializer, jobj[{property.Name}], {property}, helperValueThatsNotActuallyThere);
                    // }
                    block.Add(
                        Expression.IfThen(Expression.NotEqual(jsonProp, Expression.Constant(null)),
                            Expression.Call(
                            value,
                            property.PropertyInfo.SetMethod,
                            Expression.Convert(
                                ExpressionUtils.Replace((JsonSerializer s, JObject j, object existingValue) =>
                                    Deserialize(s, j[property.Name], property, existingValue),
                                    serializer, jobj,
                                    property.Populate ? 
                                        (Expression)Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object)) :
                                        Expression.Constant(null, typeof(object))),
                                property.Type)
                    )));

                    if (checkEV)
                    {
                        // encryptedValuesReader.AssertEnd();
                        block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));
                    }
                }
            }

            // close encrypted values
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));

            block.Add(value);


            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object, EncryptedValuesReader>>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value }, block),
                    typeof(object)).OptimizeConstants(),
                jobj, serializer, valueParam, encryptedValuesReader);
            return ex.Compile();
        }

        private static void Serialize(JsonSerializer serializer, JsonWriter writer, ViewModelPropertyMap property, object value)
        {
            if (property.JsonConverter != null && property.JsonConverter.CanWrite && property.JsonConverter.CanConvert(property.Type))
            {
                property.JsonConverter.WriteJson(writer, value, serializer);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        private static object Deserialize(JsonSerializer serializer, JToken jtoken, ViewModelPropertyMap property, object existingValue)
        {
            if (property.JsonConverter != null && property.JsonConverter.CanRead && property.JsonConverter.CanConvert(property.Type))
            {
                return property.JsonConverter.ReadJson(jtoken.CreateReader(), property.Type, existingValue, serializer);
            }
            else if (existingValue != null && property.Populate)
            {
                if (jtoken.Type == JTokenType.Null)
                {
                    return null;
                }
                else if (jtoken.Type == JTokenType.Object)
                {
                    serializer.Converters.OfType<ViewModelJsonConverter>().First().Populate((JObject) jtoken, serializer, existingValue);
                    return existingValue;
                }
                else
                {
                    serializer.Populate(jtoken.CreateReader(), existingValue);
                    return existingValue;
                }
            }
            else
            {
                if (property.Type.GetTypeInfo().IsValueType && jtoken.Type == JTokenType.Null)
                {
                    return Activator.CreateInstance(property.Type);
                }
                else
                {
                    return serializer.Deserialize(jtoken.CreateReader(), property.Type);
                }
            }
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var encryptedValuesWriter = Expression.Parameter(typeof(EncryptedValuesWriter), "encryptedValuesWriter");
            var isPostback = Expression.Parameter(typeof(bool), "isPostback");
            var value = Expression.Variable(Type, "value");

            // curly brackets are used for variables and methods from the scope of this factory method
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // encryptedValuesWriter.WriteStartObject();
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));

            // encryptedValuesWriter.Nest();
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.IncrementIndex), Type.EmptyTypes));
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes));

            // writer.WritePropertyName("$type");
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$type"), writer));

            // serializer.Serialize(writer, value.GetType().FullName)
            block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, string t) => s.Serialize(w, t), serializer, writer, Expression.Constant(Type.GetTypeHash())));

            // go through all properties that should be serialized
            foreach (var property in Properties)
            {
                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.IncrementIndex), Type.EmptyTypes));
                var endPropertyLabel = Expression.Label("end_property_" + property.Name);
                var options = new Dictionary<string, object>();
                if (property.TransferToClient && property.PropertyInfo.GetMethod != null)
                {
                    if (property.TransferFirstRequest != property.TransferAfterPostback)
                    {
                        if (property.ViewModelProtection != ProtectMode.None)
                        {
                            throw new DotvvmPropertySerializationException("Property sent only on selected requests cannot use viewModel protection.", property);
                        }

                        Expression condition = isPostback;
                        if (property.TransferAfterPostback) condition = Expression.Not(condition);
                        block.Add(Expression.IfThen(condition, Expression.Goto(endPropertyLabel)));
                    }

                    // (object)value.{property.PropertyInfo.Name}
                    var prop = Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object));

                    if (property.ViewModelProtection == ProtectMode.EncryptData ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        // encryptedValuesWriter.Value((object)value.{property.PropertyInfo.Name});
                        block.Add(
                            Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Value), Type.EmptyTypes, prop));
                    }

                    if (property.ViewModelProtection == ProtectMode.None ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        var checkEV = property.ViewModelProtection == ProtectMode.None &&
                            CanContainEncryptedValues(property.Type);
                        if (checkEV)
                        {
                            // encryptedValuesWriter.Nest();
                            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes));
                        }

                        // writer.WritePropertyName({property.Name});
                        block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes,
                            Expression.Constant(property.Name)));

                        // serializer.Serialize(serializer, writer, {property}, (object)value.{property.PropertyInfo.Name});
                        block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, object v) => Serialize(s, w, property, v), serializer, writer, prop));

                        if (checkEV)
                        {
                            // encryption is worthless if the property is not being transfered both ways 
                            // therefore ClearEmptyNest throws exception if the property contains encrypted values
                            if (!property.IsFullyTransfered())
                            {
                                // encryptedValuesWriter.ClearEmptyNest();
                                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.ClearEmptyNest), Type.EmptyTypes));
                            }
                            else
                            {
                                // encryptedValuesWriter.End();
                                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));
                            }
                        }
                    }

                    if (!property.TransferToServer)
                    {
                        // write an instruction into a viewmodel that the property may not be posted back
                        options["doNotPost"] = true;
                    }
                    else if (property.TransferToServerOnlyInPath)
                    {
                        options["pathOnly"] = true;
                    }
                }
                else if (property.TransferToServer)
                {
                    // render empty property options - we need to create the observable on the client, however we don't transfer the value
                    options["doNotUpdate"] = true;
                }
                
                if (property.ClientExtenders.Any())
                {
                    options[CLIENT_EXTENDERS_KEY] = property.ClientExtenders.ToArray();
                }

                AddTypeOptions(options, property);
                
                block.Add(Expression.Label(endPropertyLabel));
                if (options.Any())
                {
                    GenerateOptionsBlock(block, property, options, writer);
                }
            }

            // writer.WriteEndObject();
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // encryptedValuesWriter.End();
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool>>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValuesWriter, isPostback);
            return ex.Compile();
        }

        private void GenerateOptionsBlock(IList<Expression> block,  ViewModelPropertyMap property, Dictionary<string, object> options, ParameterExpression writer)
        {
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(property.Name + "$options"), writer));
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartObject(), writer));
            foreach (var option in options)
            {
                block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(option.Key), writer));
                switch (option.Key)
                {
                    case CLIENT_EXTENDERS_KEY:
                    {
                        // declare 'clientExtenders' as the array of objects
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartArray(), writer));
                        foreach (var extender in property.ClientExtenders)
                        {
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartObject(), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName("name"), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(extender.Name), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName("parameter"), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(extender.Parameter), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

                        }
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndArray(), writer));
                        break;
                    }
                    default:
                        // legacy code - direct { property : value }
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(option.Value), writer));
                        break;
                }
            }
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));
        }

        private void AddTypeOptions(Dictionary<string, object> options, ViewModelPropertyMap property)
        {
            if (property.TransferToClient || property.TransferToServer)
            {
                if ((property.Type == typeof(DateTime) || property.Type == typeof(DateTime?)) && property.JsonConverter == null) // TODO: allow customization using attributes
                {
                    options["isDate"] = true;
                }
                else if (property.Type.IsNumericType())
                {
                    options["type"] = property.Type.Name.ToLower();
                }
                else if (Nullable.GetUnderlyingType(property.Type)?.IsNumericType() == true)
                {
                    options["type"] = Nullable.GetUnderlyingType(property.Type).Name.ToLower() + "?";
                }
            }
        }

        /// <summary>
        /// Determines whether type can contain encrypted fields
        /// </summary>
        private bool CanContainEncryptedValues(Type type)
        {
            return !(
                // primitives can't contain encrypted fields
                type.GetTypeInfo().IsPrimitive ||
                type.GetTypeInfo().IsEnum ||
                type == typeof(string)
           );
        }

    }

}
