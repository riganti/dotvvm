using DotVVM.VS2015Extension.DotvvmPageWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense
{
    public class CompletionRefreshHandler : IDisposable
    {
        public static readonly CompletionRefreshHandler instance = new CompletionRefreshHandler();

        private object locker = new object();

        private DateTime nextRefreshDate;

        private Thread backgroundThread;

        public CompletionRefreshHandler()
        {
            backgroundThread = new Thread(WorkerLoop) { IsBackground = true };
            backgroundThread.Start();
        }

        ~CompletionRefreshHandler()
        {
            Dispose(false);
        }

        public event EventHandler RefreshCompletion;

        public static CompletionRefreshHandler Instance
        {
            get { return instance; }
        }

        public void NotifyRefreshNeeded()
        {
            lock (locker)
            {
                nextRefreshDate = DateTime.UtcNow.AddSeconds(2);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void WorkerLoop()
        {
            while (true)
            {
                Thread.Sleep(2000);

                var refreshNeeded = false;
                lock (locker)
                {
                    refreshNeeded = nextRefreshDate > DateTime.UtcNow;
                    nextRefreshDate = DateTime.MinValue;
                }

                if (refreshNeeded)
                {
                    try
                    {
                        if (RefreshCompletion != null)
                        {
                            RefreshCompletion(this, EventArgs.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(new Exception("Cannot perform the refresh operation!", ex));
                    }
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (backgroundThread != null && backgroundThread.IsAlive)
            {
                backgroundThread.Abort();
            }
        }
    }
}