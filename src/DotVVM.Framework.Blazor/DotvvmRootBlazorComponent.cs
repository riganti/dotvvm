using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace DotVVM.Framework.Blazor
{
    public class DotvvmRootBlazorComponent: IComponent
    {
        private readonly Dictionary<string, Action<RenderTreeBuilder>> views;
        public IReadOnlyDictionary<string, Action<RenderTreeBuilder>> Views => this.views;

        public DotvvmRootBlazorComponent((string file, Action<RenderTreeBuilder> render)[] views)
        {
            Console.WriteLine("Yay, running ctor");
            this.views = views.ToDictionary(a => a.file, a => a.render);
            CurrentView = views.First().file;
        }
        public string CurrentView { get; set; }

        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            Console.WriteLine("Yay, running render");
            var render = views[CurrentView];
            render(builder);
        }
    }
}
