namespace DotVVM.Framework.ViewModel;

/// <summary>
/// Marker interface instructing DotVVM to treat the type as a primitive type.
/// The type is required to have a static TryParse(string, [IFormatProvider,] out T) method and expected to implement ToString() method which is compatible with the TryParse method.
/// Primitive types are then serialized as string in client-side view models.
/// </summary>
public interface IDotvvmPrimitiveType { }
