using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace DotVVM.Framework.Blazor
{
    public delegate void RenderFunctionDelegate(RenderTreeBuilder builder);
    public class FileNameAndRenderFunctionTuple
    {
        public FileNameAndRenderFunctionTuple(string fileName, RenderFunctionDelegate render)
        {
            this.FileName = fileName;
            this.Render = render;
        }
        public string FileName { get; }
        public RenderFunctionDelegate Render { get; }

    }
    public class DotvvmRootBlazorComponent : IComponent
    {
        private readonly Dictionary<string, RenderFunctionDelegate> views;
        public IReadOnlyDictionary<string, RenderFunctionDelegate> Views => this.views;

        public DotvvmRootBlazorComponent(FileNameAndRenderFunctionTuple[] views)
        {
            Console.WriteLine("Yay, running ctor");
            this.views = views.ToDictionary(a => a.FileName, a => a.Render);
            CurrentView = views.First().FileName;
        }
        public string CurrentView { get; set; }

        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            Console.WriteLine("Yay, running render");
            var render = views[CurrentView];
            render(builder);
        }

        private RenderHandle renderHandle;
        public virtual void Init(RenderHandle handle)
        {
            this.renderHandle = handle;
        }
        public virtual void SetParameters(ParameterCollection parameters)
        {

        }
    }
}
