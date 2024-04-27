namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for row (item) insert feature.
    /// </summary>
    public interface IRowInsertOptions
    {
    }


    /// <summary>
    /// <see cref="IRowInsertOptions" /> which may contain one inserted row.
    /// </summary>
    public interface ISingleRowInsertOptions<out T>: IRowInsertOptions
        where T: class
    {
        T? InsertedRow { get; }
    }
}
