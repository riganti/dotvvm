namespace DotVVM.Framework.Controls
{
    public enum GridViewDataSetCommandType
    {
        /// <summary>
        /// The <see cref="GridViewDataSetBindingProvider" /> will create command bindings which set the sorting/paging options and let the user handle data loading in the PreRender phase.
        /// </summary>
        Default,
        /// <summary>
        /// The commands returned by <see cref="GridViewDataSetBindingProvider" /> utilize the LoadData function provided in the <see cref="GridViewDataSetBindingProvider.LoadDataDelegate"/> JS parameter.
        /// </summary>
        LoadDataDelegate
    }
}
