namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for row (item) edit feature.
    /// </summary>
    public class RowEditOptions : IRowEditOptions
    {
        /// <summary>
        /// Gets or sets the name of a property that uniquely identifies a row. (row ID, primary key, etc.)
        /// </summary>
        public string PrimaryKeyPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the value of a <see cref="PrimaryKeyPropertyName"/> property for the row that is being edited.
        /// </summary>
        public object EditRowId { get; set; }

    }
}