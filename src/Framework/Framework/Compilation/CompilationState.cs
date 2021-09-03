namespace DotVVM.Framework.Compilation
{
    public enum CompilationState
    {
        None = 1,
        InProcess = 2,
        CompletedSuccessfully = 3,
        CompilationFailed = 4,
        CompilationWarning = 5,
        NonCompilable = 6
    }
}