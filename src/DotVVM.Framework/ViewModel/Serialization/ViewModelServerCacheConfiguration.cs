using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ViewModelServerCacheConfiguration
    {

        public TimeSpan CacheLifetime { get; set; } = TimeSpan.FromMinutes(5);

    }
}
