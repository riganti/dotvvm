using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.HotReload
{
    public class HotReloadMarkupFileLoader : IMarkupFileLoader, IDisposable
    {
        private readonly DefaultMarkupFileLoader defaultMarkupFileLoader;
        private readonly IMarkupFileChangeNotifier notifier;
        private readonly DotvvmConfiguration configuration;

        private Task notifierTask = TaskUtils.GetCompletedTask();
        private object notifierTaskLocker = new object();
        private HashSet<string> notifierTaskDirtyFiles = new HashSet<string>();

        private readonly FileSystemWatcher[] watchers;

        public HotReloadMarkupFileLoader(DefaultMarkupFileLoader defaultMarkupFileLoader, IMarkupFileChangeNotifier notifier, DotvvmConfiguration configuration)
        {
            this.defaultMarkupFileLoader = defaultMarkupFileLoader;
            this.notifier = notifier;
            this.configuration = configuration;

            watchers = new[] { "*.dothtml", "*.dotmaster", "*.dotcontrol" }
                .Select(ext => {
                    var watcher = new FileSystemWatcher();
                    watcher.IncludeSubdirectories = true;
                    watcher.Path = configuration.ApplicationPhysicalPath;
                    watcher.Filter = ext;
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    watcher.Changed += (s, a) => OnFileChanged(configuration, a.FullPath);
                    watcher.Renamed += (s, a) => {
                        // VS doesn't update the actual file, it writes in the temp file, moves the old file away, and then renames the temp file to the original file
                        if (!string.Equals(Path.GetExtension(a.FullPath), ".tmp", StringComparison.OrdinalIgnoreCase))
                        {
                            OnFileChanged(configuration, a.FullPath);
                        }
                    };
                    watcher.EnableRaisingEvents = true;
                    return watcher;
                })
                .ToArray();
        }

        private void OnFileChanged(DotvvmConfiguration configuration, string fullPath)
        {
            // recompile view on the background so the refresh is faster
            Task.Factory.StartNew(() =>
            {
                // cannot use DI - there is cyclic dependency
                try
                {
                    var controlBuilderFactory = configuration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();
                    controlBuilderFactory.GetControlBuilder(fullPath);
                }
                catch (Exception)
                {
                    // ignore errors - this is triggered on the background so the subsequent HTTP request from the browser will have the page already compiled
                }
            });

            // notify about the changes
            lock (notifierTaskLocker)
            {
                notifierTaskDirtyFiles.Add(fullPath);

                if (notifierTask.IsCompleted)
                {
                    // do not notify faster than once per 500 ms
                    notifierTask = Task.Delay(500).ContinueWith(_ =>
                    {
                        lock (notifierTaskLocker)
                        {
                            notifier.NotifyFileChanged(notifierTaskDirtyFiles.ToList());
                            notifierTaskDirtyFiles.Clear();
                        }
                    });
                }
            }
        }

        public MarkupFile? GetMarkup(DotvvmConfiguration configuration, string virtualPath) => defaultMarkupFileLoader.GetMarkup(configuration, virtualPath);

        public string GetMarkupFileVirtualPath(IDotvvmRequestContext context) => defaultMarkupFileLoader.GetMarkupFileVirtualPath(context);

        public void Dispose()
        {
            foreach (var entry in watchers)
            {
                entry.Dispose();
            }
        }
    }
}
