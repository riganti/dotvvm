using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Redwood.Framework.Configuration;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    public class RedwoodConfigurationProvider : IDisposable
    {
        private ConcurrentDictionary<string, FileChangeTracker> changeTrackers = new ConcurrentDictionary<string, FileChangeTracker>();
        private CachedValue<string, RedwoodConfiguration> cache = new CachedValue<string, RedwoodConfiguration>();
        private IVsFileChangeEx fileChangeService;

        public event EventHandler WorkspaceChanged;

        public RedwoodConfigurationProvider()
        {
            fileChangeService = ServiceProvider.GlobalProvider.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
        }

        public RedwoodConfiguration GetConfiguration(Project project)
        {
            var applicationRootDirectory = CompletionHelper.GetProjectPath(project);
            var configurationFilePath = Path.Combine(applicationRootDirectory, "redwood.json");

            changeTrackers.GetOrAdd(configurationFilePath, CreateChangeTracker);

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

        private FileChangeTracker CreateChangeTracker(string fileName)
        {
            var tracker = new FileChangeTracker(fileChangeService, fileName);
            tracker.StartFileChangeListeningAsync();
            tracker.UpdatedOnDisk += (sender, args) => OnWorkspaceChanged();
            return tracker;
        }

        private void OnWorkspaceChanged()
        {
            cache.ClearCachedValues();
            if (WorkspaceChanged != null)
            {
                WorkspaceChanged(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            foreach (var tracker in changeTrackers.Values)
            {
                tracker.Dispose();
            }
        }
    }
}