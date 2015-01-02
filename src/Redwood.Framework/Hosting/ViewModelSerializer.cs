using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;
using Redwood.Framework.ViewModel;
using System.Text;
using System.IO;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.Hosting
{
    public class ViewModelSerializer : IViewModelSerializer
    {

        private CommandResolver commandResolver = new CommandResolver();
        private RedwoodConfiguration configuration;

        public ViewModelSerializer(RedwoodConfiguration configuration)
        {
            this.configuration = configuration;
        }


        /// <summary>
        /// Serializes the view model for the client.
        /// </summary>
        public string SerializeViewModel(object viewModel, RedwoodView view, string csrfToken)
        {
            // serialize the ViewModel
            var serializer = new JsonSerializer();
            serializer.Converters.Add(
                new ViewModelJsonConverter(new ViewModelProtectionHelper(configuration.Security, serializer)));
            var writer = new JTokenWriter();
            serializer.Serialize(writer, viewModel);

            // save the control state
            var walker = new ViewModelJTokenControlTreeWalker(writer.Token, view);
            walker.ProcessControlTree(walker.SaveControlState);

            // TODO: persist CSRF token

            return writer.Token.ToString();
        }


        /// <summary>
        /// Populates the view model from the data received from the request.
        /// </summary>
        public void PopulateViewModel(object viewModel, RedwoodView view, string serializedPostData, out Action invokedCommand, out string csrfToken)
        {
            // get properties
            var data = JObject.Parse(serializedPostData);
            var path = data["currentPath"].Values<string>().ToArray();
            var command = data["command"].Value<string>();
            var controlUniqueId = data["controlUniqueId"].Value<string>();

            // populate the ViewModel
            var serializer = new JsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter(new ViewModelProtectionHelper(configuration.Security, serializer));
            serializer.Converters.Add(viewModelConverter);
            viewModelConverter.Populate(data["viewModel"] as JObject, serializer, viewModel);
            
            // load the control state
            var walker = new ViewModelJTokenControlTreeWalker(data["viewModel"], view);
            walker.ProcessControlTree(walker.LoadControlState);

            // TODO: Output CSRF token
            csrfToken = null;

            // find the command target
            if (!string.IsNullOrEmpty(controlUniqueId)) {
                var target = view.FindControl(controlUniqueId);
                if (target == null) {
                    throw new Exception(string.Format("The control with ID '{0}' was not found!", controlUniqueId));
                }
                invokedCommand = commandResolver.GetFunction(target, view, viewModel, path, command);
            }
            else {
                invokedCommand = commandResolver.GetFunction(view, viewModel, path, command);
            }
        }
    }
}