namespace DotVVM.Framework.Compilation
{
    public enum ViewCompilationMode
    {
        /// <summary>
        /// Compilation will be done when do markup is first needed.
        /// </summary>
        Lazy,
        /// <summary>
        /// Compilation will run during application startup. 
        /// </summary>
        Precompilation,
        /// <summary> 
        /// Compilation will run during application startup. Markup will be compiled in parallel. 
        /// </summary>
        ParallelPrecompilation,
    }
}
