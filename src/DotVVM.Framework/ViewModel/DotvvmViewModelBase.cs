using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel
{
    public class DotvvmViewModelBase : IDotvvmViewModel
    {
        private IDotvvmRequestContext _context;

        [JsonIgnore]
        public IDotvvmRequestContext Context
        {
            get { return _context; }
            set
            {
                _context = value;
                foreach (var c in GetChildViewModels())
                {
                    c.Context = value;
                }
            }
        }

        async Task IDotvvmViewModel.Init()
        {
            await Init();
            var dotvvmViewModels = GetChildViewModels();
            foreach (var childViewModel in dotvvmViewModels)
            {
                await childViewModel.Init();
            }
        }

        async Task IDotvvmViewModel.Load()
        {
            await Load();
            foreach (var childViewModel in GetChildViewModels())
            {
                await childViewModel.Load();
            }
        }

        async Task IDotvvmViewModel.PreRender()
        {
            await PreRender();
            foreach (var childViewModel in GetChildViewModels())
            {
                await childViewModel.PreRender();
            }
        }

        public virtual Task Init()
            => Task.CompletedTask;

        public virtual Task Load()
            => Task.CompletedTask;

        public virtual Task PreRender()
            => Task.CompletedTask;

        protected virtual IEnumerable<IDotvvmViewModel> GetChildViewModels()
        {
            // PERF: precompile ViewModels getter
            var thisType = GetType();
            var properties = ChildViewModelsCache.GetChildViewModelsProperties(thisType).Select(p => (IDotvvmViewModel)p.GetValue(this, null));
            var collection = ChildViewModelsCache.GetChildViewModelsCollection(thisType).SelectMany(p => (IEnumerable<IDotvvmViewModel>)p.GetValue(this, null) ?? new IDotvvmViewModel[0]);

            return properties.Concat(collection).ToArray();
        }
    }
}