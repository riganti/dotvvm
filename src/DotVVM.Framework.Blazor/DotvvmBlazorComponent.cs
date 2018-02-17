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
        public virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}
