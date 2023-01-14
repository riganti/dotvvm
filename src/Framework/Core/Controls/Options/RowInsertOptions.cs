namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for row (item) insert feature.
    /// </summary>
    /// <typeparam name="T">The type of inserted row.</typeparam>
    public class RowInsertOptions<T> : IRowInsertOptions
        where T : class, new()
    {
        /// <summary>
        /// Gets or sets the row to be inserted to data source.
        /// </summary>
        public T? InsertedRow { get; set; }
    }
}
