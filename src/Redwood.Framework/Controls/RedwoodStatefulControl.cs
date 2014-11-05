using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.ViewModel;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A base class for controls that need its own control state.
    /// </summary>
    public abstract class RedwoodStatefulControl : RedwoodBindableControl, IViewModelSerializationHandler
    {

        /// <summary>
        /// Gets the state of the control.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public Dictionary<string, string> ControlState { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodStatefulControl"/> class.
        /// </summary>
        public RedwoodStatefulControl()
        {
            ControlState = new Dictionary<string, string>();
        }





        /// <summary>
        /// Generates the control ID automatically.
        /// </summary>
        private string AutoGenerateID()
        {
            // TODO: auto ID generation
            throw new InvalidOperationException("The stateful control must have its ID!");
        }

        /// <summary>
        /// Called when the viewmodel node is deserialized.
        /// </summary>
        public void OnNodeDeserialized(object viewModel, object dataContext)
        {
            if (string.IsNullOrEmpty(ID))
            {
                AutoGenerateID();
            }

            // TODO: implement state persistence
        }

        /// <summary>
        /// Called when the viewmodel node is serialized.
        /// </summary>
        public void OnSerializingNode(object viewModelMap, object dataContext)
        {
            if (string.IsNullOrEmpty(ID))
            {
                AutoGenerateID();
            }

            // TODO: implement state persistence
        }
    }
}
