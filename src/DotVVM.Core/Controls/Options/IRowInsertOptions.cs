namespace DotVVM.Framework.Controls
{

    /// <summary>
    /// Represents a settings for row insert feature.
    /// </summary>
    /// <typeparam name="T">The type of inserted row.</typeparam>
    public interface IRowInsertOptions<T> : IRowInsertOptions
        where T : new()
    {
        /// <summary>
        /// Gets or sets the row to be inserted to data source.
        /// </summary>
        new T InsertedRow { get; set; }
    }

    /// <summary>
    /// Represents a settings for row insert feature.
    /// </summary>
    public interface IRowInsertOptions
    {
        /// <summary>
        /// Gets or sets the row to be inserted to data source.
        /// </summary>
        object InsertedRow { get; }
    }
}