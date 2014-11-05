namespace Redwood.Framework.ViewModel
{
    public interface IViewModelSerializationHandler
    {

        /// <summary>
        /// Called when the viewmodel node is deserialized.
        /// </summary>
        void OnNodeDeserialized(object viewModel, object dataContext);

        /// <summary>
        /// Called when the viewmodel node is serialized.
        /// </summary>
        void OnSerializingNode(object viewModel, object dataContext);

    }
}