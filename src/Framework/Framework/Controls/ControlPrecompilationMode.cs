namespace DotVVM.Framework.Controls;

public enum ControlPrecompilationMode
{
    /// <summary> Never attempt precompilation. </summary>
    Never,
    /// <summary> Attempt precompilation whenever it's possible (the control is CompositeControl and there are no resource bindings in properties which can't handle bindings). If exception is thrown by the control, it is ignored and precompilation is skipped. </summary>
    IfPossibleAndIgnoreExceptions,
    /// <summary> Attempt precompilation whenever it's possible (the control is CompositeControl and there are no resource bindings in properties which can't handle bindings). If exception is thrown by the control, compilation fails. </summary>
    IfPossible,
    /// <summary> Always try to precompile the control and fail compilation when it's not possible. </summary>
    Always,
    /// <summary> Always precompile this controls and do that while styles are being processed. This will allow other styles to match onto the generated controls. </summary>
    InServerSideStyles
}
