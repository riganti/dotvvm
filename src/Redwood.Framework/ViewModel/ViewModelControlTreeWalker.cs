using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.ViewModel
{
    /// <summary>
    /// Goes through the control tree and maps it to the JToken with the ViewModel representation
    /// </summary>
    public class ViewModelControlTreeWalker
    {
        private JToken viewModelToken;
        private RedwoodView root;


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelControlTreeWalker"/> class.
        /// </summary>
        public ViewModelControlTreeWalker(JToken viewModelToken, RedwoodView root)
        {
            this.viewModelToken = viewModelToken;
            this.root = root;
        }


        /// <summary>
        /// Processes the control tree.
        /// </summary>
        public void ProcessControlTree(Action<JToken, RedwoodControl> action)
        {
            var evaluator = new ViewModelJTokenEvaluator(viewModelToken);
            ProcessControlTreeCore(evaluator, viewModelToken, root, action);
        }

        /// <summary>
        /// Processes the control tree.
        /// </summary>
        private void ProcessControlTreeCore(ViewModelJTokenEvaluator evaluator, JToken viewModelToken, RedwoodControl control, Action<JToken, RedwoodControl> action)
        {
            action(viewModelToken, control);

            // if there is a DataContext binding, locate the correct token
            ValueBindingExpression binding;
            if (control is RedwoodBindableControl && 
                (binding = ((RedwoodBindableControl)control).GetBinding(RedwoodBindableControl.DataContextProperty, false) as ValueBindingExpression) != null)
            {
                viewModelToken = evaluator.Evaluate(binding.Expression);
            }

            if (viewModelToken is JObject || viewModelToken is JArray)
            {
                // go through all children
                foreach (var child in control.Children)
                {
                    ProcessControlTreeCore(evaluator, viewModelToken, child, action);
                }
            }
        }

        /// <summary>
        /// Loads the state of the control.
        /// </summary>
        public void LoadControlState(JToken viewModelToken, RedwoodControl control)
        {
            if (control is RedwoodBindableControl && ((RedwoodBindableControl)control).RequiresControlState)
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
                            ((RedwoodBindableControl)control).ControlState[item.Name] = item.Value.ToObject(type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the state of the control.
        /// </summary>
        public void SaveControlState(JToken viewModelToken, RedwoodControl control)
        {
            if (control is RedwoodBindableControl && ((RedwoodBindableControl)control).RequiresControlState)
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
                foreach (var item in ((RedwoodBindableControl)control).ControlState.Where(v => v.Value != null))
                {
                    controlState.Add(item.Key, JToken.FromObject(item.Value));
                    controlState.Add(item.Key + "$type", JToken.FromObject(item.Value.GetType()));
                }
            }
        }
        
    }
}
