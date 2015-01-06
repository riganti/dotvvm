using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Configuration;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime.Filters;
using Redwood.Framework.Security;
using Redwood.Framework.ViewModel;

namespace Redwood.Framework.Runtime
{
    public class DefaultViewModelSerializer : IViewModelSerializer
    {

        private CommandResolver commandResolver = new CommandResolver();

        private RedwoodConfiguration configuration;
        private readonly IViewModelProtector viewModelProtector;

        public DefaultViewModelSerializer(RedwoodConfiguration configuration, IViewModelProtector viewModelProtector)
        {
            this.configuration = configuration;
            this.viewModelProtector = viewModelProtector;
        }


        /// <summary>
        /// Serializes the view model for the client.
        /// </summary>
        public string SerializeViewModel(RedwoodRequestContext context, RedwoodView view)
        {
            // serialize the ViewModel
            var serializer = new JsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter() { EncryptedValues = new JArray() };
            serializer.Converters.Add(viewModelConverter);
            var writer = new JTokenWriter();
            serializer.Serialize(writer, context.ViewModel);

            // save the control state
            var walker = new ViewModelJTokenControlTreeWalker(writer.Token, view);
            walker.ProcessControlTree(walker.SaveControlState);

            // persist CSRF token
            writer.Token["$csrfToken"] = context.CsrfToken;

            // persist encrypted values
            writer.Token["$encryptedValues"] = viewModelProtector.Protect(viewModelConverter.EncryptedValues.ToString(), context);

            // create result object
            var result = new JObject();
            result["viewModel"] = writer.Token;
            result["action"] = "successfulCommand";

            return result.ToString();
        }

        /// <summary>
        /// Serializes the redirect action.
        /// </summary>
        public string SerializeRedirectAction(RedwoodRequestContext context, string url)
        {
            // create result object
            var result = new JObject();
            result["url"] = url;
            result["action"] = "redirect";
            return result.ToString();
        }


        /// <summary>
        /// Populates the view model from the data received from the request.
        /// </summary>
        public void PopulateViewModel(RedwoodRequestContext context, RedwoodView view, string serializedPostData)
        {
            // get properties
            var data = JObject.Parse(serializedPostData);
            var viewModelToken = (JObject)data["viewModel"];

            // load CSRF token
            context.CsrfToken = viewModelToken["$csrfToken"].Value<string>();

            // load encrypted values
            var encryptedValuesString = viewModelToken["$encryptedValues"].Value<string>();
            var encryptedValues = JArray.Parse(viewModelProtector.Unprotect(encryptedValuesString, context));
            
            // populate the ViewModel
            var serializer = new JsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter() { EncryptedValues = encryptedValues };
            serializer.Converters.Add(viewModelConverter);
            viewModelConverter.Populate(viewModelToken, serializer, context.ViewModel);

            // load the control state
            var walker = new ViewModelJTokenControlTreeWalker(viewModelToken, view);
            walker.ProcessControlTree(walker.LoadControlState);

        }

        /// <summary>
        /// Resolves the command for the specified post data.
        /// </summary>
        public void ResolveCommand(RedwoodRequestContext context, RedwoodView view, string serializedPostData, out ActionInfo actionInfo)
        {
            // get properties
            var data = JObject.Parse(serializedPostData);
            var path = data["currentPath"].Values<string>().ToArray();
            var command = data["command"].Value<string>();
            var controlUniqueId = data["controlUniqueId"].Value<string>();

            if (string.IsNullOrEmpty(command))
            {
                // empty command
                actionInfo = null;
            }
            else
            {
                // find the command target
                if (!string.IsNullOrEmpty(controlUniqueId))
                {
                    var target = view.FindControl(controlUniqueId);
                    if (target == null)
                    {
                        throw new Exception(string.Format("The control with ID '{0}' was not found!", controlUniqueId));
                    }
                    actionInfo = commandResolver.GetFunction(target, view, context.ViewModel, path, command);
                }
                else
                {
                    actionInfo = commandResolver.GetFunction(view, context.ViewModel, path, command);
                }
            }
        }
    }
}
