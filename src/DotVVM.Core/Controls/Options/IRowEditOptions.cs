namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for row edit feature.
    /// </summary>
    public interface IRowEditOptions
    {
        /// <summary>
        /// Gets or sets the name of property that uniquely identifies the row - unique row ID, primary key etc.
        /// </summary>
        string PrimaryKeyPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the value of PrimaryKeyPropertyName property for the row that is currently edited.
        /// </summary>
        object EditRowId { get; set; }

    }
}