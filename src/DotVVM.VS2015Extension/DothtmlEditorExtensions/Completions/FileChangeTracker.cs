using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{

    public class FileChangeTracker : IVsFileChangeEvents, IDisposable
    {
        private readonly IVsFileChangeEx fileChangeService;
        private readonly string filePath;
        private bool disposed;
        
        private Lazy<uint> watchedFileId;

        public event EventHandler UpdatedOnDisk;


        public FileChangeTracker(IVsFileChangeEx fileChangeService, string filePath)
        {
            this.fileChangeService = fileChangeService;
            this.filePath = filePath;
        }
        
        public void StartFileChangeListeningAsync()
        {
            watchedFileId = new Lazy<uint>(() =>
            {
                uint newCookie;
                Marshal.ThrowExceptionForHR(
                    fileChangeService.AdviseFileChange(filePath, (uint)_VSFILECHANGEFLAGS.VSFILECHG_Time, this, out newCookie));
                return newCookie;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            Task.Run(() => watchedFileId.Value, CancellationToken.None);
        }

        public void StopFileChangeListening()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("FileChangeTracker");
            }

            if (watchedFileId != null)
            {
                Marshal.ThrowExceptionForHR(fileChangeService.UnadviseFileChange(watchedFileId.Value));
                watchedFileId = null;
            }
        }

        public void Dispose()
        {
            StopFileChangeListening();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        int IVsFileChangeEvents.DirectoryChanged(string directory)
        {
            throw new NotSupportedException("We only watch files; we should never be seeing directory changes!");
        }

        int IVsFileChangeEvents.FilesChanged(uint changeCount, string[] files, uint[] changes)
        {
            var handler = UpdatedOnDisk;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }
    }
}