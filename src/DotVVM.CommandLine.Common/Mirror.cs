using System;
using System.Reflection;

namespace DotVVM.CommandLine
{
    /// <summary>
    /// Yet another set of reflection helpers.
    /// </summary>
    /// <remarks>
    /// Mirror returns a reflection, get it?
    /// </remarks>
    public static class Mirror
    {
        public static object Invoke(Type type, string name, object[] args, object? instance = null)
        {
            return type.InvokeMember(
                name: name,
                invokeAttr: BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.FlattenHierarchy
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.OptionalParamBinding
                    | BindingFlags.InvokeMethod,
                binder: Type.DefaultBinder,
                target: instance,
                args: args);
        }
    }
}
