namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for row edit feature.
    /// </summary>
    public class RowEditOptions : IRowEditOptions
    {
        /// <summary>
        /// Gets or sets the name of property that uniquely identifies the row - unique row ID, primary key etc.
        /// </summary>
        public string PrimaryKeyPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the value of PrimaryKeyPropertyName property for the row that is currently edited.
        /// </summary>
        public object EditRowId { get; set; }

    }
}