namespace DotVVM.Framework.ResourceManagement
{
    public interface IDeferrableResource
    {
        /// <summary> If `defer` attribute should be used. </summary>
        bool Defer { get; }
    }
}
