using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class DotvvmSerializationAttribute: Attribute
    {
        /// <summary>
        /// When true, DotVVM serializer will select the JSON converter based on the runtime type, instead of deciding it ahead of time.
        /// This essentially enables serialization of properties defined derived types, but does not enable derive type deserialization, unless an instance of the correct type is prepopulated into the property.
        /// By default, dynamic dispatch is enabled for abstract types (including interfaces and System.Object).
        /// </summary>
        public bool AllowDynamicDispatch
        {
            get => _allowDynamicDispatch ?? false;
            set => _allowDynamicDispatch = value;
        }
        internal bool? _allowDynamicDispatch;
        /// <summary> See <see cref="AllowDynamicDispatch" /> </summary>
        public bool AllowsDynamicDispatch(bool defaultValue) => _allowDynamicDispatch ?? defaultValue;

        /// <summary>
        /// Normally, DotVVM uses its own JSON converter to serialize viewmodels.
        /// If you want to serialize this type using the default System.Text.Json serializer, set this property to true.
        /// Note the annotation isn't recursive and nested types might be again serialized using the DotVVM serializer.
        /// </summary>
        public bool DisableDotvvmConverter { get; set; }

        public static bool IsDotvvmSerializationDisabled(Type type) =>
            type.IsDefined(typeof(DotvvmSerializationAttribute), true) && type.GetCustomAttribute<DotvvmSerializationAttribute>()!.DisableDotvvmConverter;
    }
}
