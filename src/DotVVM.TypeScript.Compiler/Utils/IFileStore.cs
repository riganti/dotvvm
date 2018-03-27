using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.TypeScript.Compiler.Utils
{
    public interface IFileStore
    {
        void StoreFile(string path, string content);
        Task StoreFileAsync(string path, string content);
    }
}
