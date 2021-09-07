using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary> Object which is basically DotvvmBindableObject. This interface is useful for defining interfaces for controls, otherwise please prefer using DotvvmBindableObject directly. </summary>
    public interface IDotvvmObjectLike
    {
        /// <summary> Returns itself. This is a kinda hack which allows interfaces to inherit from almost DotvvmBindableObject </summary>
        DotvvmBindableObject Self { get; }
    }
    /// <summary> Marker interface for DotvvmBindableObject which have the specified capability. If no capability of type TCapability is defined, it will be registered automatically. </summary>
    public interface IObjectWithCapability<TCapability>: IDotvvmObjectLike
    {
    }
    public interface IDotvvmControl: IRenderable, IDotvvmObjectLike
    {
        DotvvmControlCollection Children { get; }
        ClientIDMode ClientIDMode { get; set; }
        string ID { get; set; }
        DotvvmBindableObject? Parent { get; set; }
        IEnumerable<DotvvmBindableObject> GetAllAncestors(bool includingThis = false);

        IEnumerable<DotvvmControl> GetAllDescendants(Func<DotvvmControl, bool>? enumerateChildrenCondition = null);

        IEnumerable<DotvvmBindableObject> GetLogicalChildren();

        DotvvmControl GetNamingContainer();

        DotvvmBindableObject GetRoot();

        IEnumerable<DotvvmControl> GetThisAndAllDescendants(Func<DotvvmControl, bool>? enumerateChildrenCondition = null);

        object? GetValue(DotvvmProperty property, bool inherit = true);

        bool HasOnlyWhiteSpaceContent();

        bool IsPropertySet(DotvvmProperty property, bool inherit = true);

        void SetValue(DotvvmProperty property, object value);
    }
}
