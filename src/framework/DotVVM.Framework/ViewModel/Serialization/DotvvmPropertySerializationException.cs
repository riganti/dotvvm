using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmPropertySerializationException : Exception
    {
        public ViewModelPropertyMap SerializedProperty { get; set; }

        public DotvvmPropertySerializationException(string message, ViewModelPropertyMap serializedProperty)
            : base(message)
        {
            SerializedProperty = serializedProperty;
        }

        public DotvvmPropertySerializationException(string message, Exception innerException, ViewModelPropertyMap serializedProperty)
            : base(message, innerException)
        {
            SerializedProperty = serializedProperty;
        }
    }
}
