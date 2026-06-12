namespace DotVVM.Framework.Controls
{
    /// <summary> Dataset with NoRowInsertOptions does not support the row insertion feature. </summary>
    public sealed class NoRowInsertOptions : IRowInsertOptions
    {
        public static NoRowInsertOptions Instance { get; } = new();
    }
}
