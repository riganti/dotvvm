using System.Threading.Tasks;

namespace DotVVM.TypeScript.Compiler.Utils.IO
{
    public interface IFileStore
    {
        void StoreFile(string path, string content);
        Task StoreFileAsync(string path, string content);
    }
}
