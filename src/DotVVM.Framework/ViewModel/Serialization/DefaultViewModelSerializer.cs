﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Commands;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelSerializer : IViewModelSerializer
    {
        private const string GeneralViewModelRecommendations = "Check out general viewModel recommendation at http://www.dotvvm.com/docs/tutorials/basics-viewmodels.";

        private CommandResolver commandResolver = new CommandResolver();

        private readonly IViewModelProtector viewModelProtector;
        private readonly IViewModelSerializationMapper viewModelMapper;

        public bool SendDiff { get; set; } = true;

        public Formatting JsonFormatting { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModelSerializer"/> class.
        /// </summary>
        public DefaultViewModelSerializer(DotvvmConfiguration configuration, IViewModelProtector protector, IViewModelSerializationMapper serializationMapper)
        {
            this.viewModelProtector = protector;
            this.JsonFormatting = configuration.Debug ? Formatting.Indented : Formatting.None;
            this.viewModelMapper = serializationMapper;
        }

        /// <summary>
        /// Serializes the view model.
        /// </summary>
        public string SerializeViewModel(IDotvvmRequestContext context)
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
        public void BuildViewModel(IDotvvmRequestContext context)
        {
            // serialize the ViewModel
            var serializer = CreateJsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, viewModelMapper, context.Services) {
                UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
            };
            serializer.Converters.Add(viewModelConverter);
            var writer = new JTokenWriter();
            try
            {
                serializer.Serialize(writer, context.ViewModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not serialize viewModel of type { context.ViewModel.GetType().Name }. Serialization failed at property { writer.Path }. {GeneralViewModelRecommendations}", ex);
            }

            // persist CSRF token
            writer.Token["$csrfToken"] = context.CsrfToken;

            // persist encrypted values
            if (viewModelConverter.EncryptedValues.Count > 0)
                writer.Token["$encryptedValues"] = viewModelProtector.Protect(viewModelConverter.EncryptedValues.ToString(Formatting.None), context);

            // serialize validation rules
            bool useClientSideValidation = context.Configuration.ClientSideValidation;
            var validationRules = useClientSideValidation ?
                SerializeValidationRules(viewModelConverter) :
                null;

            // create result object
            var result = new JObject();
            result["viewModel"] = writer.Token;
            result["url"] = context.HttpContext?.Request?.Url?.PathAndQuery;
            result["virtualDirectory"] = context.HttpContext?.Request?.PathBase?.Value?.Trim('/') ?? "";
            if (context.ResultIdFragment != null)
            {
                result["resultIdFragment"] = context.ResultIdFragment;
            }
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
            if (validationRules?.Count > 0) result["validationRules"] = validationRules;

            context.ViewModelJson = result;
        }

        public string BuildStaticCommandResponse(IDotvvmRequestContext context, object result)
        {
            var serializer = CreateJsonSerializer();
            var viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, viewModelMapper, context.Services) {
                UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
            };
            serializer.Converters.Add(viewModelConverter);
            var writer = new JTokenWriter();
            try
            {
                serializer.Serialize(writer, result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not serialize viewModel of type { context.ViewModel.GetType().Name }. Serialization failed at property { writer.Path }. {GeneralViewModelRecommendations}", ex);
            }
            return writer.Token.ToString(JsonFormatting);
        }

        public static JsonSerializerSettings CreateDefaultSettings()
        {
            var s = new JsonSerializerSettings() {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };
            s.Converters.Add(new DotvvmDateTimeConverter());
            s.Converters.Add(new StringEnumConverter());
            return s;
        }

        protected virtual JsonSerializer CreateJsonSerializer() => CreateDefaultSettings().Apply(JsonSerializer.Create);

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
                        resourceObj[resource.Name] = JValue.CreateString(resource.GetRenderedTextCached(context));
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

                foreach (var property in map.Properties)
                {
                    if (property.ValidationRules.Count > 0 && property.ClientValidationRules.Any())
                        rule[property.Name] = JToken.FromObject(property.ClientValidationRules);
                }
                if (rule.Count > 0) validationRules[map.Type.GetTypeHash()] = rule;
            }
            return validationRules;
        }

        /// <summary>
        /// Serializes the redirect action.
        /// </summary>
        public static string GenerateRedirectActionResponse(string url, bool replace, bool allowSpa)
        {
            // create result object
            var result = new JObject();
            result["url"] = url;
            result["action"] = "redirect";
            if (replace) result["replace"] = true;
            if (allowSpa) result["allowSpa"] = true;
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
        public void PopulateViewModel(IDotvvmRequestContext context, string serializedPostData)
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
                viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, viewModelMapper, context.Services, JObject.Parse(viewModelProtector.Unprotect(encryptedValuesString, context)));
            }
            else viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, viewModelMapper, context.Services);

            // get validation path
            context.ModelState.ValidationTargetPath = data.SelectToken("additionalData.validationTargetPath")?.Value<string>();

            // populate the ViewModel
            var serializer = CreateJsonSerializer();
            serializer.Converters.Add(viewModelConverter);
            try
            {
                viewModelConverter.Populate(viewModelToken.CreateReader(), serializer, context.ViewModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not deserialize viewModel of type { context.ViewModel.GetType().Name }. {GeneralViewModelRecommendations}", ex);
            }
        }

        /// <summary>
        /// Resolves the command for the specified post data.
        /// </summary>
        public ActionInfo ResolveCommand(IDotvvmRequestContext context, DotvvmView view)
        {
            // get properties
            var data = context.ReceivedViewModelJson ?? throw new NotSupportedException("Could not find ReceivedViewModelJson in request context.");
            var path = data["currentPath"].Values<string>().ToArray();
            var command = data["command"].Value<string>();
            var controlUniqueId = data["controlUniqueId"]?.Value<string>();
            var args = data["commandArgs"]?.ToObject<object[]>() ?? new object[0];

            // empty command
            if (string.IsNullOrEmpty(command)) return null;

            // find the command target
            if (!string.IsNullOrEmpty(controlUniqueId))
            {
                var target = view.FindControlByUniqueId(controlUniqueId);
                if (target == null)
                {
                    throw new Exception(string.Format("The control with ID '{0}' was not found!", controlUniqueId));
                }
                return commandResolver.GetFunction(target, view, context, path, command, args);
            }
            else
            {
                return commandResolver.GetFunction(view, context, path, command, args);
            }
        }

        /// <summary>
        /// Adds the post back updated controls.
        /// </summary>
        public void AddPostBackUpdatedControls(IDotvvmRequestContext context, IEnumerable<(string name, string html)> postBackUpdatedControls)
        {
            var result = new JObject();
            foreach (var (controlId, html) in postBackUpdatedControls)
            {
                result[controlId] = JValue.CreateString(html);
            }
            context.ViewModelJson["updatedControls"] = result;
        }
    }
}
