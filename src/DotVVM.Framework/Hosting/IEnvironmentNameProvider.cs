namespace DotVVM.Framework.Hosting
{
    public interface IEnvironmentNameProvider
    {
        /// <summary>
        /// Returns the name of the hosting environment (eg. Development, Production).
        /// </summary>
        string GetEnvironmentName(IDotvvmRequestContext context);
    }
}