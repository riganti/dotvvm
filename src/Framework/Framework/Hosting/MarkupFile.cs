using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Hosting
{
    public class MarkupFile
    {
        protected bool Equals(MarkupFile? other)
        {
            return other != null && string.Equals(FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase) && LastWriteDateTimeUtc.Equals(other.LastWriteDateTimeUtc);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FullPath != null ? FullPath.ToLowerInvariant().GetHashCode() : 0) * 397) ^ LastWriteDateTimeUtc.GetHashCode();
            }
        }

        public const string ViewFileExtension = ".dothtml";



        public Func<string> ReadContent { get; private set; }

        public string FileName { get; private set; }

        public string FullPath { get; private set; }

        public DateTime LastWriteDateTimeUtc { get; private set; }


        public MarkupFile(string fileName, string fullPath)
        {
            FileName = fileName;
            FullPath = fullPath;
            LastWriteDateTimeUtc = File.GetLastWriteTimeUtc(fullPath);
            ReadContent = () =>
            {
                // retry logic because of Hot reload
                Exception? lastException = null;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        return File.ReadAllText(fullPath);
                    }
                    catch (FileNotFoundException)
                    {
                        break;
                    }
                    catch (IOException ex)
                    {
                        lastException = ex;
                        Thread.Sleep(20);
                    }
                }

                throw new DotvvmCompilationException($"Cannot load the markup file '{fileName}'.", lastException);
            };
        }

        public MarkupFile(string fileName, string fullPath, string contents)
        {
            FileName = fileName;
            FullPath = fullPath;
            ReadContent = () => contents;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((MarkupFile)obj);
        }
    }
}
