using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel
{
    public class DotvvmViewModelBase : IDotvvmViewModel
    {
        [JsonIgnore]
        public IDotvvmRequestContext Context { get; set; }


        async Task IDotvvmViewModel.Init()
        {
            await this.Init();
            var dotvvmViewModels = GetChildViewModels();
            foreach (var childViewModel in dotvvmViewModels)
            {
                await childViewModel.Init();
            }
        }

        async Task IDotvvmViewModel.Load()
        {
            await this.Load();
            foreach (var childViewModel in GetChildViewModels())
            {
                await childViewModel.Load();
            }
        }

        async Task IDotvvmViewModel.PreRender()
        {
            await this.PreRender();
            foreach (var childViewModel in GetChildViewModels())
            {
                await childViewModel.PreRender();
            }
        }
        public virtual Task Init()
        {
            return TaskUtils.GetCompletedTask();
        }

        public virtual Task Load()
        {
            return TaskUtils.GetCompletedTask();
        }

        public virtual Task PreRender()
        {
            return TaskUtils.GetCompletedTask();
        }

        protected virtual IEnumerable<IDotvvmViewModel> GetChildViewModels()
        {
            var thisType = this.GetType();
            var properties = ChildViewModelsCache.GetChildViewModelsProperties(thisType).Select(p => (IDotvvmViewModel)p.GetValue(this, null));
            var collection = ChildViewModelsCache.GetChildViewModelsCollection(thisType).SelectMany(p => (IEnumerable<IDotvvmViewModel>)p.GetValue(this, null));

            return properties.Concat(collection).ToArray();
        }
    }
}