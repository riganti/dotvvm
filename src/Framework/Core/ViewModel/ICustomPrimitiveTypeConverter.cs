namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Represents a converter which can convert the value from string or other type that may appear in route parameters collection.
    /// </summary>
    public interface ICustomPrimitiveTypeConverter
    {
        /// <summary>
        /// Converts the value from its client-side representation or other type that may appear in route parameters collection to the registered custom primitive type.
        /// </summary>
        object? ToCustomPrimitiveType(object? value);

        /// <summary>
        /// Converts the value from the registered custom primitive type to its client-side representation.
        /// </summary>
        object? FromCustomPrimitiveType(object? value);
    }
}
