using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Controls;
using System.IO;

namespace DotVVM.Framework.Runtime
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
        public DefaultViewModelSerializer(DotvvmConfiguration configuration)
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
        public string SerializeViewModel(DotvvmRequestContext context)
        {
            if (SendDiff && context.ReceivedViewModelJson != null && context.ViewModelJson["viewModel"] != null)
            {
                context.ViewModelJson["viewModelDiff"] = JsonUtils.Diff((JObject)context.ReceivedViewModelJson["viewModel"], (JObject)context.ViewModelJson["viewModel"], true);
                context.ViewModelJson.Remove("viewModel");
            }
            return context.ViewModelJson.ToString(JsonFormatting);
        }

        /// <summary>
        /// Builds the view model for the client.
        /// </summary>
        public void BuildViewModel(DotvvmRequestContext context)
        {
            // serialize the ViewModel
            var serializer = CreateJsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter()
            {
                UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
            };
            serializer.Converters.Add(viewModelConverter);
            var writer = new JTokenWriter();
            serializer.Serialize(writer, context.ViewModel);

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
            result["url"] = context.OwinContext.Request.Uri.PathAndQuery;
            result["virtualDirectory"] = DotvvmMiddleware.GetVirtualDirectory(context.OwinContext);
            if (context.IsPostBack || context.IsSpaRequest)
            {
                result["action"] = "successfulCommand";
                var renderedResources = new HashSet<string>(context.ReceivedViewModelJson?["renderedResources"]?.Values<string>() ?? new string[] { });
                result["resources"] = BuildResourcesJson(context, rn => !renderedResources.Contains(rn));
            }
            else
            {
                result["renderedResources"] = JArray.FromObject(context.ResourceManager.RequiredResources);
            }
            // TODO: do not send on postbacks
            if (validationRules.Count > 0) result["validationRules"] = validationRules;

            context.ViewModelJson = result;
        }

        protected virtual JsonSerializer CreateJsonSerializer()
        {
            var s = new JsonSerializer()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };
            //s.Converters.Add(new StringEnumConverter());
            return s;
        }

        public JObject BuildResourcesJson(IDotvvmRequestContext context, Func<string, bool> predicate)
        {
            var manager = context.ResourceManager;
            var resourceObj = new JObject();
            foreach (var resource in manager.GetNamedResourcesInOrder())
            {
                if (predicate(resource.Name))
                {
                    using (var str = new StringWriter())
                    {
                        var w = new HtmlWriter(str, context);
                        resource.Resource.Render(w);
                        resourceObj[resource.Name] = JValue.CreateString(str.ToString());
                    }
                }
            }
            return resourceObj;
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
        public static string GenerateRedirectActionResponse(string url)
        {
            // create result object
            var result = new JObject();
            result["url"] = url;
            result["action"] = "redirect";
            return result.ToString(Formatting.None);
        }

        /// <summary>
        /// Serializes the validation errors in case the viewmodel was not valid.
        /// </summary>
        public string SerializeModelState(IDotvvmRequestContext context)
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
        public void PopulateViewModel(DotvvmRequestContext context, string serializedPostData)
        {

            // get properties
            var data = context.ReceivedViewModelJson = JObject.Parse(serializedPostData);
            var viewModelToken = (JObject)data["viewModel"];

            // load CSRF token
            context.CsrfToken = viewModelToken["$csrfToken"].Value<string>();

            ViewModelJsonConverter viewModelConverter;
            if (viewModelToken["$encryptedValues"] != null)
            {
                // load encrypted values
                var encryptedValuesString = viewModelToken["$encryptedValues"].Value<string>();
                viewModelConverter = new ViewModelJsonConverter(JObject.Parse(viewModelProtector.Unprotect(encryptedValuesString, context)));
            }
            else viewModelConverter = new ViewModelJsonConverter();

            // get validation path
            context.ModelState.ValidationTargetPath = data["validationTargetPath"].Value<string>();

            // populate the ViewModel
            var serializer = CreateJsonSerializer();
            serializer.Converters.Add(viewModelConverter);
            viewModelConverter.Populate(viewModelToken, serializer, context.ViewModel);
        }

        /// <summary>
        /// Resolves the command for the specified post data.
        /// </summary>
        public void ResolveCommand(DotvvmRequestContext context, DotvvmView view, string serializedPostData, out ActionInfo actionInfo)
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
        public void AddPostBackUpdatedControls(DotvvmRequestContext context)
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
