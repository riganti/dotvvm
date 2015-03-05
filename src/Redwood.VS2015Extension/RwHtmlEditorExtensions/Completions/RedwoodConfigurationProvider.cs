using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;
using Redwood.Framework.Configuration;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    public class RedwoodConfigurationProvider
    {
        private readonly DTE2 dte;

        private CachedValue<string, RedwoodConfiguration> cache = new CachedValue<string, RedwoodConfiguration>();

        public RedwoodConfigurationProvider(DTE2 dte)
        {
            this.dte = dte;
        }

        public RedwoodConfiguration GetConfiguration(Project project)
        {
            var applicationRootDirectory = CompletionHelper.GetProjectPath(project);
            var configurationFilePath = Path.Combine(applicationRootDirectory, "redwood.json");

            var configuration = RedwoodConfiguration.CreateDefault();
            try
            {
                var fileContents = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(fileContents, configuration);
            }
            catch (Exception ex)
            {
                // TODO: report that the configuration cannot be loaded
            }
            configuration.ApplicationPhysicalPath = applicationRootDirectory;

            return configuration;
        }
    }
}