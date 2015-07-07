using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Goes through the control tree and maps it to the JToken with the ViewModel representation
    /// </summary>
    public class ViewModelJTokenControlTreeWalker : ControlTreeWalker<JToken>
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelJTokenControlTreeWalker"/> class.
        /// </summary>
        public ViewModelJTokenControlTreeWalker(JToken viewModel, DotvvmControl root)
            : base(viewModel, root)
        {
        }

        /// <summary>
        /// Creates the expression evaluator.
        /// </summary>
        protected override IExpressionEvaluator<JToken> CreateEvaluator(JToken viewModel)
        {
            return new ViewModelJTokenEvaluator(viewModel);
        }


        /// <summary>
        /// Determines whether the walker shoulds the process children for current viewModel value.
        /// </summary>
        protected override bool ShouldProcessChildren(JToken viewModel)
        {
            return viewModel is JObject || viewModel is JArray;
        }

        /// <summary>
        /// Loads the state of the control.
        /// </summary>
        public void LoadControlState(JToken viewModelToken, DotvvmControl control)
        {
            if (control is DotvvmBindableControl && ((DotvvmBindableControl)control).RequiresControlState)
            {
                // find the control state
                var controlStateCollection = viewModelToken["$controlState"];
                if (controlStateCollection != null)
                {
                    control.EnsureControlHasId();
                    var controlState = controlStateCollection[control.ID] as JObject;
                    if (controlState != null)
                    {
                        // fill the collection with property values
                        foreach (var item in controlState.Properties().Where(p => !p.Name.EndsWith("$type")))
                        {
                            var type = Type.GetType(controlState[item.Name + "$type"].Value<string>());
                            ((DotvvmBindableControl)control).ControlState[item.Name] = item.Value.ToObject(type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the state of the control.
        /// </summary>
        public void SaveControlState(JToken viewModelToken, DotvvmControl control)
        {
            if (control is DotvvmBindableControl && ((DotvvmBindableControl)control).RequiresControlState)
            {
                // find the control state collection
                var controlStateCollection = viewModelToken["$controlState"];
                if (controlStateCollection == null)
                {
                    controlStateCollection = new JObject();
                    ((JObject)viewModelToken).Add("$controlState", controlStateCollection);
                }

                // find the control state item
                control.EnsureControlHasId();
                var controlState = controlStateCollection[control.ID] as JObject;
                if (controlState == null)
                {
                    controlState = new JObject();
                    ((JObject)controlStateCollection).Add(control.ID, controlState);
                }

                // serialize the property values
                foreach (var item in ((DotvvmBindableControl)control).ControlState.Where(v => v.Value != null))
                {
                    controlState.Add(item.Key, JToken.FromObject(item.Value));
                    controlState.Add(item.Key + "$type", JToken.FromObject(item.Value.GetType()));
                }
            }
        }

    }
}
