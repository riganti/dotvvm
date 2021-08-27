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
        public string? TagPrefix
        {
            get => _tagPrefix;
            set { ThrowIfFrozen(); _tagPrefix = value; }
        }
        private string? _tagPrefix;

        [JsonProperty("tagName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? TagName
        {
            get => _tagName;
            set { ThrowIfFrozen(); _tagName = value; }
        }
        private string? _tagName;

        [JsonProperty("namespace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Namespace
        {
            get => _namespace;
            set { ThrowIfFrozen(); _namespace = value; }
        }
        private string? _namespace;

        [JsonProperty("assembly", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Assembly
        {
            get => _assembly;
            set { ThrowIfFrozen(); _assembly = value; }
        }
        private string? _assembly;

        [JsonProperty("src", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Src
        {
            get => _src;
            set { ThrowIfFrozen(); _src = value; }
        }
        private string? _src;


        /// <summary>
        /// Determines whether the specified tag prefix and tag name match this rule.
        /// </summary>
        public bool IsMatch(string tagPrefix, string tagName)
        {
            return tagPrefix.Equals(TagPrefix, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(TagName) || tagName.Equals(TagName, StringComparison.OrdinalIgnoreCase));
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

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                FreezableUtils.Error(nameof(DotvvmControlConfiguration));
        }
        public void Freeze()
        {
            this.isFrozen = true;
        }
    }


}
