namespace DotVVM.Framework.ResourceManagement
{
    public interface IDeferableResource
    {
        /// <summary> If `defer` attribute should be used. </summary>
        bool Defer { get; }
    }
}
