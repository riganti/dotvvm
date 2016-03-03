using System;
using System.Collections.Concurrent;

namespace DotVVM.Framework.Configuration
{
    public class ServiceLocator
    {

        private ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();
        private ConcurrentDictionary<Type, object> singletonInstances = new ConcurrentDictionary<Type, object>(); 


        public void RegisterTransient<T>(Func<T> factory)
        {
            factories[typeof (T)] = () => factory();
        }

        public void RegisterSingleton<T>(Func<T> factory)
        {
            factories[typeof (T)] = CreateSingletonFactory(factory);
        }



        public T GetService<T>()
        {
            return (T)factories[typeof (T)]();
        }




        private Func<object> CreateSingletonFactory<T>(Func<T> factory)
        {
            return () => singletonInstances.GetOrAdd(typeof (T), t => factory());
        }
        
    }
}
