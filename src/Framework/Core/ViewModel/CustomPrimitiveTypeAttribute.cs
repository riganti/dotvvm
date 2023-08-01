using System;

namespace DotVVM.Framework.ViewModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Obsolete("Implement IDotvvmPrimitiveType.", error: true)]
public class CustomPrimitiveTypeAttribute : Attribute
{
}
