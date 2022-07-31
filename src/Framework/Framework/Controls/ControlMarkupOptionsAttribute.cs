using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary> Controls various aspects of how this control behaves in dothtml markup files. </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ControlMarkupOptionsAttribute : Attribute
    {
        /// <summary> When false, adding children to this control in a markup file will be an error. </summary>
        public bool AllowContent { get; set; } = true;

        /// <summary> Name of the DotvvmProperty where all child nodes will be placed. When null <see cref="DotvvmControl.Children" /> collection is used. If the property is not a collection type, only one child control will be allowed. </summary>
        public string? DefaultContentProperty { get; set; }

        /// <summary> When set, the control will be evaluated only once during view compilation, instead of execting it for every request. It only work for <see cref="CompositeControl" />s. </summary>
        public ControlPrecompilationMode Precompile { get; set; } = ControlPrecompilationMode.Never;
    }

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
}
