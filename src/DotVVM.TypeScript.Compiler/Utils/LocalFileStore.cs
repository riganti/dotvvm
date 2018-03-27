using System.IO;
using System.Threading.Tasks;

namespace DotVVM.TypeScript.Compiler.Utils
{
    public class LocalFileStore : IFileStore
    {
        public void StoreFile(string path, string content)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(content);
                }
            } 
        }

        public async Task StoreFileAsync(string path, string content)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    await streamWriter.WriteAsync(content);
                }
            }
        }
    }
}
