namespace DotVVM.Framework.Controls
{
    public interface IRowEditGridViewDataSet
    {
        string PrimaryKeyPropertyName { get; set; }
        object EditRowId { get; set; }
    }
}