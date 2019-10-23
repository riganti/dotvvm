using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{

    /// <summary>
    /// Can get physical location of the file for debugging purposes. In that directory can be located associated source maps and based on file will be the resource refreshed.
    /// </summary>
    public interface IDebugFileLocalLocation: ILocalResourceLocation
    {
        string GetFilePath(IDotvvmRequestContext context);
    }
}
