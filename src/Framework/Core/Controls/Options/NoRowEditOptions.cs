namespace DotVVM.Framework.Controls
{

    /// <summary> Dataset with NoRowEditOptions does not support the row edit feature. </summary>
    public sealed class NoRowEditOptions : IRowEditOptions
    {
        public string? PrimaryKeyPropertyName => null;

        public object? EditRowId => null;

        public static NoRowEditOptions Instance { get; } = new();
    }
}
