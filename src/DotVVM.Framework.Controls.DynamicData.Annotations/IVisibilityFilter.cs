namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Represents an attribute that defines conditions for a field to be shown or hidden.
    /// </summary>
    public interface IVisibilityFilter
    {
        /// <summary>
        /// Evaluates whether the field should be shown or hidden. If this method returns null, the next filter attribute will be evaluated.
        /// </summary>
        VisibilityMode? CanShow(IViewContext viewContext);

    }
}