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

namespace DotVVM.Diagnostics.ViewHotReload
{
    public class HotReloadMarkupFileLoader : IMarkupFileLoader, IDisposable
    {
        private readonly DefaultMarkupFileLoader defaultMarkupFileLoader;
        
        private readonly ConcurrentDictionary<string, FileSystemWatcher> watchers = new ConcurrentDictionary<string, FileSystemWatcher>();
        
        private readonly IMarkupFileChangeNotifier notifier;
        private Task notifierTask = TaskUtils.GetCompletedTask();
        private object notifierTaskLocker = new object();
        private HashSet<string> notifierTaskDirtyFiles = new HashSet<string>();

        public HotReloadMarkupFileLoader(DefaultMarkupFileLoader defaultMarkupFileLoader, IMarkupFileChangeNotifier notifier)
        {
            this.defaultMarkupFileLoader = defaultMarkupFileLoader;
            this.notifier = notifier;
        }

        public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            var markupFile = defaultMarkupFileLoader.GetMarkup(configuration, virtualPath);

            watchers.GetOrAdd(virtualPath, path =>
            {
                var fullPath = Path.Combine(configuration.ApplicationPhysicalPath, path);

                var watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(fullPath);
                watcher.Filter = Path.GetFileName(fullPath);
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.Changed += (s, a) => OnFileChanged(configuration, path);
                watcher.Renamed += (s, a) =>
                {
                    // VS doesn't update the actual file, it writes in the temp file, moves the old file away, and then renames the temp file to the original file
                    if (string.Equals(a.Name, watcher.Filter, Environment.OSVersion.Platform == PlatformID.Win32NT ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    {
                        OnFileChanged(configuration, path);
                    }
                };
                watcher.EnableRaisingEvents = true;

                return watcher;
            });

            return markupFile;
        }

        private void OnFileChanged(DotvvmConfiguration configuration, string virtualPath)
        {
            // recompile view on the background so the refresh is faster
            Task.Factory.StartNew(() =>
            {
                // cannot use DI - there is cyclic dependency
                var controlBuilderFactory = configuration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();
                controlBuilderFactory.GetControlBuilder(virtualPath);
            });

            // notify about the changes
            lock (notifierTaskLocker)
            {
                notifierTaskDirtyFiles.Add(virtualPath);

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

        public string GetMarkupFileVirtualPath(IDotvvmRequestContext context)
        {
            return defaultMarkupFileLoader.GetMarkupFileVirtualPath(context);
        }

        public void Dispose()
        {
            foreach (var entry in watchers)
            {
                entry.Value.Dispose();
            }
        }
    }
}
