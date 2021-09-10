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
        /// Application will start after compilation is done.
        /// </summary>
        DuringApplicationStart,
        
        /// <summary>
        /// Compilation will run after application started.
        /// </summary>
        AfterApplicationStart
    }
}
