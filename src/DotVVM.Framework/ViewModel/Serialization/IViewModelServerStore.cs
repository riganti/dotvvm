using System;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelServerStore
    {

        void Store(string hash, byte[] cacheData);

        byte[] Retrieve(string hash);

    }
}
