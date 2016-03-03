using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IDotvvmControl
    {
        DotvvmControlCollection Children { get; }
        ClientIDMode ClientIDMode { get; set; }
        string ID { get; set; }
        DotvvmControl Parent { get; set; }

        void EnsureControlHasId(bool autoGenerate = true);

        DotvvmControl FindControl(string id, bool throwIfNotFound = false);

        T FindControl<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl;

        IEnumerable<DotvvmControl> GetAllAncestors();

        IEnumerable<DotvvmControl> GetAllDescendants(Func<DotvvmControl, bool> enumerateChildrenCondition = null);

        IEnumerable<DotvvmBindableObject> GetLogicalChildren();

        DotvvmControl GetNamingContainer();

        DotvvmBindableObject GetRoot();

        IEnumerable<DotvvmControl> GetThisAndAllDescendants(Func<DotvvmControl, bool> enumerateChildrenCondition = null);

        object GetValue(DotvvmProperty property, bool inherit = true);

        bool HasOnlyWhiteSpaceContent();

        bool IsPropertySet(DotvvmProperty property, bool inherit = true);

        void Render(IHtmlWriter writer, IDotvvmRequestContext context);

        void SetValue(DotvvmProperty property, object value);
    }
}