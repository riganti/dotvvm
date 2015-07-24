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
using DotVVM.Framework.Configuration;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class DotvvmConfigurationProvider : IDisposable
    {
        private ConcurrentDictionary<string, FileChangeTracker> changeTrackers = new ConcurrentDictionary<string, FileChangeTracker>();
        private CachedValue<string, DotvvmConfiguration> cache = new CachedValue<string, DotvvmConfiguration>();
        private IVsFileChangeEx fileChangeService;

        public event EventHandler WorkspaceChanged;

        public DotvvmConfigurationProvider()
        {
            fileChangeService = ServiceProvider.GlobalProvider.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
        }

        public DotvvmConfiguration GetConfiguration(Project project)
        {
            var applicationRootDirectory = CompletionHelper.GetProjectPath(project);
            var configurationFilePath = Path.Combine(applicationRootDirectory, "dotvvm.json");

            changeTrackers.GetOrAdd(configurationFilePath, CreateChangeTracker);

            var configuration = DotvvmConfiguration.CreateDefault();
            try
            {
                var fileContents = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(fileContents, configuration);
            }
            catch (Exception ex)
            {
                LogService.LogError(new Exception("Cannot load the dotvvm.json configuration file!", ex));
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