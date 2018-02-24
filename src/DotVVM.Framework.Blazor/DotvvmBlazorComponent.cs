using System;
using System.Collections.Generic;
using System.Linq;
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
        }
        public virtual void SetParameters(ParameterCollection parameters)
        {

        }
    }
}
