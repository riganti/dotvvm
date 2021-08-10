using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ViewModelNesting
{
	public class NestedViewModel : DotvvmViewModelBase
	{
        public ViewModel1 ViewModel1Property { get; set; } = new ViewModel1 { ChildCount = 5 };

        [Bind(Direction.ServerToClient)]
        public ViewModel2 ViewModel2Property { get; set; } = new ViewModel2();

        public List<ViewModel2> ViewModel2Collection { get; set; } = new List<ViewModel2> { new ViewModel2(), new ViewModel2() };

        public override Task Load()
        {
            if (!ViewModel2Property.Initialized)
            {
                throw new Exception();
            }
            return base.Load();
        }

        public class ViewModel1: DotvvmViewModelBase
        {
            [Bind(Direction.None)]
            public ViewModel1[] ChildViewModels { get; set; }
            public int ChildCount { get; set; }

            public override Task Load()
            {
                ChildViewModels = Enumerable.Range(0, ChildCount)
                    .Select(i => new ViewModel1 { ChildCount = ChildCount - 1 })
                    .ToArray();
                return base.Load();
            }

            public IEnumerable<HierarchyItem> EnumerateChildren()
            {
                if (ChildViewModels == null) yield break;
                foreach (var c in ChildViewModels)
                {
                    yield return new HierarchyItem { Item = c, Offset = 0 };
                    foreach (var cc in c.EnumerateChildren())
                    {
                        cc.Offset++;
                        yield return cc;
                    }
                }
            }
            [Bind(Direction.ServerToClient)]
            public IEnumerable<HierarchyItem> AllChildren => EnumerateChildren();
        }

        public class HierarchyItem
        {
            public int Offset { get; set; }
            public ViewModel1 Item { get; set; }
        }

        public class ViewModel2: IDotvvmViewModel
        {
            [Bind(Direction.None)]
            public IDotvvmRequestContext Context { get; set; }

            public string StringProperty { get; set; }

            public bool Initialized { get; set; }
            public bool Loaded { get; set; }
            public bool PreRendered { get; set; }

            public string QueryParameterId => Context.Query["Id"]?.ToString() ?? "";

            public async Task Init()
            {
                await Task.Delay(100);
                if (!Context.Query.ContainsKey("Id")) Context.RedirectToUrl(new UriBuilder(Context.HttpContext.Request.Url.ToString()) { Query = "Id=13" }.Uri.ToString());
                Initialized = true;
            }

            public async Task Load()
            {
                await Task.Delay(50);
                Loaded = true;
            }

            public async Task PreRender()
            {
                await Task.Delay(50);
                PreRendered = true;
            }
        }
    }
}

