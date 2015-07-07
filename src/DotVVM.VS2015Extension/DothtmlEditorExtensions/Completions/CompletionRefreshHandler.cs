using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class CompletionRefreshHandler : IDisposable
    {

        public static readonly CompletionRefreshHandler instance = new CompletionRefreshHandler();

        public static CompletionRefreshHandler Instance
        {
            get { return instance; }
        }



        private object locker = new object();
        private DateTime nextRefreshDate;
        private Thread backgroundThread;

        public CompletionRefreshHandler()
        {
            backgroundThread = new Thread(WorkerLoop) { IsBackground = true };
            backgroundThread.Start();
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

        public event EventHandler RefreshCompletion;

        public void NotifyRefreshNeeded()
        {
            lock (locker)
            {
                nextRefreshDate = DateTime.UtcNow.AddSeconds(5);
            }
        }

        ~CompletionRefreshHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
