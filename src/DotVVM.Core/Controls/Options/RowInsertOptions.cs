namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for row insert feature.
    /// </summary>
    /// <typeparam name="T">The type of inserted row.</typeparam>
    public class RowInsertOptions<T> : IRowInsertOptions<T> where T : new()
    {
        /// <inheritdoc />
        public T InsertedRow { get; set; }
        
        object IRowInsertOptions.InsertedRow => InsertedRow;
    }
}