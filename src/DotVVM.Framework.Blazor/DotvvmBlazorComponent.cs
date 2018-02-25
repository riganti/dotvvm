using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework;
using DotVVM.Framework.Controls;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace DotVVM.Framework.Blazor
{
    public class DotvvmBlazorComponent: IComponent
    {
        private RenderHandle renderHandle;
        public virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
        }

        public virtual void Init(RenderHandle handle)
        {
            this.renderHandle = handle;

            var dataContextProperty = this.GetType().GetProperty("DataContext");
            if (dataContextProperty != null)
            {
                dataContextProperty.SetValue(this, Activator.CreateInstance(dataContextProperty.PropertyType));
            }
        }

        public virtual void SetParameters(ParameterCollection parameters)
        {
            this.renderHandle.Render(BuildRenderTree);
        }
    }
}
