namespace DotVVM.Framework.Configuration
{
    /// <summary>
    /// Represents a converter which can convert the value from string or other type that may appear in route parameters collection.
    /// </summary>
    public interface ICustomPrimitiveTypeConverter
    {
        /// <summary>
        /// Converts the value from string or other type that may appear in route parameters collection.
        /// </summary>
        object? Convert(object? value);
    }
}
