﻿namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for row (item) insert feature.
    /// </summary>
    /// <typeparam name="T">The type of inserted row.</typeparam>
    public interface IRowInsertOptions<T> : IRowInsertOptions
        where T : class, new()
    {
        /// <summary>
        /// Gets or sets the row to be inserted to data source. Null means that row insertion is not activated.
        /// </summary>
        new T? InsertedRow { get; set; }
    }

    /// <summary>
    /// Represents settings for row (item) insert feature.
    /// </summary>
    public interface IRowInsertOptions
    {
        /// <summary>
        /// Gets or sets the row to be inserted into data source. Null means that row insertion is not activated.
        /// </summary>
        object? InsertedRow { get; }
    }
}
