namespace DotVVM.Framework.Compilation.Styles
{
    public enum StyleOverrideOptions
    {
        /// <summary> If the property is already set, nothing happens. </summary>
        Ignore,
        /// <summary> If the property is already set, it overrides the original value. </summary>
        Overwrite,
        /// <summary> If the property is already set, try to append them together. </summary>
        Append,
        /// <summary> If the property is already set, try to append them together, but the original value should be the second one. </summary>
        Prepend
    }
}
