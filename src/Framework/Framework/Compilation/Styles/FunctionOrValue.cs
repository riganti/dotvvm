using System;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Compilation.Styles
{
    /// <summary> A helper class for an optionally computed value that can still be used for debug strings. </summary>
    internal abstract class FunctionOrValue<TInput, TValue>
    {
        public abstract TValue Invoke(TInput input);
        public abstract bool TryGetValue([MaybeNullWhen(false)] out TValue result);

        public string DebugString(Func<TValue, string> f, string defaultStr = "A computed value") =>
            TryGetValue(out var v) ? f(v) : defaultStr;

        public override string ToString() => TryGetValue(out var v) ? "" + v : "A computed value";

        public class ValueCase : FunctionOrValue<TInput, TValue>
        {
            public TValue Value { get; }
            public ValueCase(TValue value) { Value = value; }
            public override TValue Invoke(TInput input) => Value;
            public override bool TryGetValue([MaybeNullWhen(false)] out TValue result)
            {
                result = Value;
                return true;
            }
        }
        public class FunctionCase : FunctionOrValue<TInput, TValue>
        {
            public Func<TInput, TValue> Function { get; }
            public FunctionCase(Func<TInput, TValue> function) { Function = function; }

            public override TValue Invoke(TInput input) => Function(input);
            public override bool TryGetValue([MaybeNullWhen(false)] out TValue result)
            {
                result = default;
                return false;
            }
        }
    }
}
