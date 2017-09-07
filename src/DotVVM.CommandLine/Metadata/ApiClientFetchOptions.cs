using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Metadata
{
    public class ApiClientFetchOptions
    {

        [DefaultValue("same-origin")]
        [JsonProperty("credentials")]
        public string Credentials { get; set; } = "same-origin";

    }
}