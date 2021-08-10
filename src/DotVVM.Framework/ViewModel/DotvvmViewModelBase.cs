using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Validation;
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
            foreach (var childViewModel in GetChildViewModels())
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
            => Task.FromResult(0);

        public virtual Task Load()
            => Task.FromResult(0);

        public virtual Task PreRender()
            => Task.FromResult(0);

        protected virtual IEnumerable<IDotvvmViewModel> GetChildViewModels()
        {
            // PERF: precompile ViewModels getter
            var thisType = GetType();
            var properties = ChildViewModelsCache.GetChildViewModelsProperties(thisType).Select(p => (IDotvvmViewModel)p.GetValue(this, null));
            var collection = ChildViewModelsCache.GetChildViewModelsCollection(thisType).SelectMany(p => (IEnumerable<IDotvvmViewModel>)p.GetValue(this, null) ?? new IDotvvmViewModel[0]);

            return properties.Concat(collection).Where(c => c != null).ToArray();
        }


        internal void ExecuteOnViewModelRecursive(Action<IDotvvmViewModel> action)
        {
            // TODO: viewmodel hierarchy should be managed in a separate class - we need something like IHierarchicalDotvvmViewModel and IViewModelHierarchyManager
            action(this);
            foreach (var childViewModel in GetChildViewModels())
            {
                if (childViewModel is DotvvmViewModelBase dotvvmChildViewModel)
                {
                    dotvvmChildViewModel.ExecuteOnViewModelRecursive(action);
                }
                else
                {
                    action(childViewModel);
                }
            }
        } 
    }
}