using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Controls
{
    /// <summary> Options for the <see cref="KnockoutHelper.GenerateClientPostBackExpression(string, Binding.Expressions.ICommandBinding, DotvvmBindableObject, PostbackScriptOptions)" /> method. </summary>
    public sealed record PostbackScriptOptions
    {
        /// <summary>If true, the command invocation will be wrapped in window.setTimeout with timeout 0. This is necessary for some event handlers, when the handler is invoked before the change is actually applied. Default is false</summary>
        public bool? UseWindowSetTimeout { get; init; }
        /// <summary>Return value of the event handler. If set to false, the script will also include event.stopPropagation(). Null means that `null` will be returned.</summary>
        public bool? ReturnValue { get; init; }
        /// <summary> When true, the invocation is suppressed while viewmodel is being updated. Default is false. </summary>
        public bool? IsOnChange { get; init; }
        /// <summary>Javascript variable where the sender element can be found. Set to $element when in knockout binding.</summary>
        public CodeParameterAssignment? ElementAccessor { get; init; }
        /// <summary>Javascript variable current knockout binding context can be found. By default, `ko.contextFor({elementAccessor})` is used</summary>
        public CodeParameterAssignment? KoContext { get; init; }
        /// <summary>Javascript expression returning an array of command arguments.</summary>
        public CodeParameterAssignment? CommandArgs { get; init; }
        /// <summary>When set to false, postback handlers will not be invoked for this command. Default is true.</summary>
        public bool? AllowPostbackHandlers { get; init; }
        /// <summary>Javascript expression returning <see href="https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal">AbortSignal</see> which can be used to cancel the postback (it's a JS variant of CancellationToken). </summary>
        public CodeParameterAssignment? AbortSignal { get; init; }
        public Func<CodeSymbolicParameter, CodeParameterAssignment>? ParameterAssignment { get; init; }

        /// <param name="useWindowSetTimeout">If true, the command invocation will be wrapped in window.setTimeout with timeout 0. This is necessary for some event handlers, when the handler is invoked before the change is actually applied.</param>
        /// <param name="returnValue">Return value of the event handler. If set to false, the script will also include event.stopPropagation()</param>
        /// <param name="isOnChange">If set to true, the command will be suppressed during updating of view model. This is necessary for certain onChange events, if we don't want to trigger the command when the view model changes.</param>
        /// <param name="elementAccessor">Javascript variable where the sender element can be found. Set to $element when in knockout binding, and this when in JS event.</param>
        /// <param name="koContext">Javascript variable current knockout binding context can be found. By default, `ko.contextFor({elementAccessor})` is used</param>
        /// <param name="commandArgs">Javascript expression returning an array of command arguments.</param>
        /// <param name="allowPostbackHandlers">When set to false, postback handlers will not be invoked for this command.</param>
        /// <param name="abortSignal">Javascript expression returning <see href="https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal">AbortSignal</see> which can be used to cancel the postback (it's a JS variant of CancellationToken). </param>
        public PostbackScriptOptions(
            bool? useWindowSetTimeout = null,
            bool? returnValue = false,
            bool? isOnChange = null,
            string? elementAccessor = null,
            CodeParameterAssignment? koContext = null,
            CodeParameterAssignment? commandArgs = null,
            bool? allowPostbackHandlers = null,
            CodeParameterAssignment? abortSignal = null,
            Func<CodeSymbolicParameter, CodeParameterAssignment>? parameterAssignment = null)
        {
            this.UseWindowSetTimeout = useWindowSetTimeout;
            this.ReturnValue = returnValue;
            this.IsOnChange = isOnChange;
            this.ElementAccessor = elementAccessor is null ? (CodeParameterAssignment?)null : new CodeParameterAssignment(elementAccessor, OperatorPrecedence.Max);
            this.KoContext = koContext;
            this.CommandArgs = commandArgs;
            this.AllowPostbackHandlers = allowPostbackHandlers;
            this.AbortSignal = abortSignal;
            this.ParameterAssignment = parameterAssignment;
        }

        /// <summary> Default postback options, optimal for placing the script into a `onxxx` event attribute. </summary>
        public static readonly PostbackScriptOptions JsEvent = new PostbackScriptOptions(
            elementAccessor: "this",
            koContext: new CodeParameterAssignment(new ParametrizedCode(["ko.contextFor(", ")"], [JavascriptTranslator.CurrentElementParameter.ToInfo()], OperatorPrecedence.Max))
        );
        public static readonly PostbackScriptOptions KnockoutBinding = new PostbackScriptOptions(
            elementAccessor: "$element",
            koContext: new CodeParameterAssignment("$context", OperatorPrecedence.Max, isGlobalContext: true)
        );

        public PostbackScriptOptions WithDefaults(PostbackScriptOptions? defaults)
        {
            if (defaults is null) return this;
            return this with {
                UseWindowSetTimeout = UseWindowSetTimeout ?? defaults.UseWindowSetTimeout,
                // ReturnValue is ignored on purpose, because it is set to false in the constructor
                IsOnChange = IsOnChange ?? defaults.IsOnChange,
                ElementAccessor = ElementAccessor ?? defaults.ElementAccessor,
                KoContext = KoContext ?? defaults.KoContext,
                CommandArgs = CommandArgs ?? defaults.CommandArgs,
                AllowPostbackHandlers = AllowPostbackHandlers ?? defaults.AllowPostbackHandlers,
                AbortSignal = AbortSignal ?? defaults.AbortSignal,
                ParameterAssignment = ParameterAssignment ?? defaults.ParameterAssignment,
            };
        }

        public PostbackScriptOptions Override(PostbackScriptOptions? overrides) =>
            overrides is null ? this : overrides.WithDefaults(this);

        public override string ToString()
        {
            var fields = new List<string>();
            if (UseWindowSetTimeout is {}) fields.Add($"useWindowSetTimeout: {UseWindowSetTimeout}");
            if (ReturnValue != false) fields.Add($"returnValue: {(ReturnValue == true ? "true" : "null")}");
            if (IsOnChange is {}) fields.Add($"isOnChange: {IsOnChange}");
            if (ElementAccessor.ToString() != "this") fields.Add($"elementAccessor: \"{ElementAccessor}\"");
            if (KoContext != null) fields.Add($"koContext: \"{KoContext}\"");
            if (CommandArgs != null) fields.Add($"commandArgs: \"{CommandArgs}\"");
            if (AllowPostbackHandlers is {}) fields.Add($"allowPostbackHandlers: {AllowPostbackHandlers}");
            if (AbortSignal != null) fields.Add($"abortSignal: \"{AbortSignal}\"");
            if (ParameterAssignment != null) fields.Add($"parameterAssignment: \"{ParameterAssignment}\"");
            return new StringBuilder("new PostbackScriptOptions(").Append(string.Join(", ", fields.ToArray())).Append(")").ToString();
        }
    }
}
