using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmControlConfiguration
    {

        [JsonProperty("tagPrefix", Required = Required.Always)]
        public string TagPrefix { get; set; }

        [JsonProperty("tagName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TagName { get; set; }

        [JsonProperty("namespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Namespace { get; set; }

        [JsonProperty("assembly", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Assembly { get; set; }

        [JsonProperty("src", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Src { get; set; }


        /// <summary>
        /// Determines whether the specified tag prefix and tag name match this rule.
        /// </summary>
        public bool IsMatch(string tagPrefix, string tagName)
        {
            return tagPrefix == TagPrefix && (string.IsNullOrEmpty(TagName) || tagName == TagName);
        }

        /// <summary>
        /// Validates the rule.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(TagPrefix))
            {
                throw new Exception("The TagPrefix must not be empty and must not contain non-alphanumeric characters!");       // TODO: exception handling
            }

            if (string.IsNullOrEmpty(TagName))
            {
                if (!string.IsNullOrEmpty(Src) || string.IsNullOrEmpty(Namespace) || string.IsNullOrEmpty(Assembly))
                {
                    throw new Exception(@"Invalid combination of parameters found in the configuration. Path markup/controls. Only these combinations of parameters are valid:
{""tagPrefix"": ""value"", ""namespace"": ""value"", ""assembly"": ""value""} or 
{""tagPrefix"": ""value"", ""tagName"": ""value"", ""src"": ""value""}");       // TODO: exception handling    
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Src) || !string.IsNullOrEmpty(Namespace) || !string.IsNullOrEmpty(Assembly))
                {
                    throw new Exception(@"Invalid combination of parameters found in the configuration. Path markup/controls. Only these combinations of parameters are valid:
{""tagPrefix"": ""value"", ""namespace"": ""value"", ""assembly"": ""value""} or 
{""tagPrefix"": ""value"", ""tagName"": ""value"", ""src"": ""value""}");       // TODO: exception handling    
                }
            }
        }
    }


}