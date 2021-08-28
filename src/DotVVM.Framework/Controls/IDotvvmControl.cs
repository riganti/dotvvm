using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IDotvvmControl: IRenderable
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
