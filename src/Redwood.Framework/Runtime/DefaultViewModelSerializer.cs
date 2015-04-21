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
using Redwood.Framework.Utils;

namespace Redwood.Framework.Runtime
{
    public class DefaultViewModelSerializer : IViewModelSerializer
    {

        private CommandResolver commandResolver = new CommandResolver();

        private readonly IViewModelProtector viewModelProtector;

        public bool SendDiff { get; set; }

        public Formatting JsonFormatting { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModelSerializer"/> class.
        /// </summary>
        public DefaultViewModelSerializer(RedwoodConfiguration configuration)
        {
            this.viewModelProtector = configuration.ServiceLocator.GetService<IViewModelProtector>();
            this.JsonFormatting = configuration.Debug ? Formatting.Indented : Formatting.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModelSerializer"/> class.
        /// </summary>
        public DefaultViewModelSerializer(IViewModelProtector viewModelProtector)
        {
            this.viewModelProtector = viewModelProtector;
        }

        /// <summary>
        /// Serializes the view model.
        /// </summary>
        public string SerializeViewModel(RedwoodRequestContext context)
        {
            if (SendDiff && context.ReceivedViewModelJson != null && context.ViewModelJson["viewModel"] != null)
            {
                bool changed;
                context.ViewModelJson["viewModelDiff"] = JsonUtils.Diff((JObject)context.ReceivedViewModelJson["viewModel"], (JObject)context.ViewModelJson["viewModel"], out changed, true);
                if (!changed) context.ViewModelJson.Remove("viewModelDiff");
                context.ViewModelJson.Remove("viewModel");
            }
            return context.ViewModelJson.ToString(JsonFormatting);
        }

        /// <summary>
        /// Builds the view model for the client.
        /// </summary>
        public void BuildViewModel(RedwoodRequestContext context, RedwoodView view)
        {
            // serialize the ViewModel
            var serializer = new JsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter()
            {
                EncryptedValues = new JArray(),
                UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
            };
            serializer.Converters.Add(viewModelConverter);
            var writer = new JTokenWriter();
            serializer.Serialize(writer, context.ViewModel);

            // save the control state
            var walker = new ViewModelJTokenControlTreeWalker(writer.Token, view);
            walker.ProcessControlTree(walker.SaveControlState);

            // persist CSRF token
            writer.Token["$csrfToken"] = context.CsrfToken;

            // persist encrypted values
            if (viewModelConverter.EncryptedValues.Count > 0)
                writer.Token["$encryptedValues"] = viewModelProtector.Protect(viewModelConverter.EncryptedValues.ToString(Formatting.None), context);

            // serialize validation rules
            var validationRules = SerializeValidationRules(viewModelConverter);

            // create result object
            var result = new JObject();
            result["viewModel"] = writer.Token;
            result["action"] = "successfulCommand";
            result["url"] = context.OwinContext.Request.Uri.PathAndQuery;
            result["virtualDirectory"] = context.Configuration.VirtualDirectory;
            if (validationRules.Count > 0) result["validationRules"] = validationRules;

            context.ViewModelJson = result;
        }

        /// <summary>
        /// Serializes the validation rules.
        /// </summary>
        private JObject SerializeValidationRules(ViewModelJsonConverter viewModelConverter)
        {
            var validationRules = new JObject();
            foreach (var map in viewModelConverter.UsedSerializationMaps)
            {
                var rule = new JObject();
                foreach (var property in map.Properties.Where(p => p.ClientValidationRules.Any()))
                {
                    rule[property.Name] = JToken.FromObject(property.ClientValidationRules);
                }
                if (rule.Count > 0) validationRules[map.Type.ToString()] = rule;
            }
            return validationRules;
        }

        /// <summary>
        /// Serializes the redirect action.
        /// </summary>
        public string GenerateRedirectActionResponse(string url)
        {
            // create result object
            var result = new JObject();
            result["url"] = url;
            result["action"] = "redirect";
            return result.ToString(JsonFormatting);
        }

        /// <summary>
        /// Serializes the validation errors in case the viewmodel was not valid.
        /// </summary>
        public string SerializeModelState(RedwoodRequestContext context)
        {
            // create result object
            var result = new JObject();
            result["modelState"] = JArray.FromObject(context.ModelState.Errors);
            result["action"] = "validationErrors";
            return result.ToString(JsonFormatting);
        }


        /// <summary>
        /// Populates the view model from the data received from the request.
        /// </summary>
        /// <returns></returns>
        public void PopulateViewModel(RedwoodRequestContext context, RedwoodView view, string serializedPostData)
        {
            var viewModelConverter = new ViewModelJsonConverter();

            // get properties
            var data = context.ReceivedViewModelJson = JObject.Parse(serializedPostData);
            var viewModelToken = (JObject)data["viewModel"];

            // load CSRF token
            context.CsrfToken = viewModelToken["$csrfToken"].Value<string>();

            if (viewModelToken["$encryptedValues"] != null)
            {
                // load encrypted values
                var encryptedValuesString = viewModelToken["$encryptedValues"].Value<string>();
                viewModelConverter.EncryptedValues = JArray.Parse(viewModelProtector.Unprotect(encryptedValuesString, context));
            }
            else viewModelConverter.EncryptedValues = new JArray();

            // get validation path
            context.ModelState.ValidationTargetPath = data["validationTargetPath"].Value<string>();

            // populate the ViewModel
            var serializer = new JsonSerializer();
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
                    actionInfo = commandResolver.GetFunction(target, view, context, path, command);
                }
                else
                {
                    actionInfo = commandResolver.GetFunction(view, context, path, command);
                }
            }
        }

        /// <summary>
        /// Adds the post back updated controls.
        /// </summary>
        public void AddPostBackUpdatedControls(RedwoodRequestContext context)
        {
            var result = new JObject();
            foreach (var control in context.PostBackUpdatedControls)
            {
                result[control.Key] = JValue.CreateString(control.Value);
            }
            context.ViewModelJson["updatedControls"] = result;
        }
    }
}
