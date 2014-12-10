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

namespace Redwood.Framework.Hosting
{
    public class ViewModelSerializer : IViewModelSerializer
    {

        private CommandResolver commandResolver = new CommandResolver();

        /// <summary>
        /// Serializes the view model for the client.
        /// </summary>
        public string SerializeViewModel(object viewModel, RedwoodView view)
        {
            // TODO: add the control state to the view model map

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new ViewModelJsonConverter());
            var sb = new StringBuilder();
            using(var jw = new StringWriter(sb))
            {
                serializer.Serialize(jw, viewModel);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Populates the view model from the data received from the request.
        /// </summary>
        public void PopulateViewModel(object viewModel, RedwoodView view, string serializedPostData, out Action invokedCommand)
        {
            var data = JObject.Parse(serializedPostData);
            var path = data["currentPath"].Values<string>().ToArray();
            var command = data["command"].Value<string>();

            // populate the view model map
            var serializer = new JsonSerializer();
            serializer.Populate(data["viewModel"].CreateReader(), viewModel);

            // TODO: restore control state

            // resolve the command
            invokedCommand = commandResolver.GetFunction(viewModel, path, command);
        }
    }
}