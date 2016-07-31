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
            object temp;
            singletonInstances.TryRemove(typeof(T), out temp); //removes old service instance and allows to create new one
        }

        public T GetService<T>()
        {
            var service = (T)factories[typeof (T)]();
            if (service == null) throw new ArgumentException($"Constructor for service {typeof(T)} returned null.");
            return service;
        }

        public T GetService<T>(T @default)
        {
            if (!HasService<T>()) return @default;

            var service =  (T) factories[typeof(T)]();
            return service == null ? @default : service;
        }

        public bool HasService<T>()
        {
            return factories.ContainsKey(typeof(T));
        }


        private Func<object> CreateSingletonFactory<T>(Func<T> factory)
        {
            return () => singletonInstances.GetOrAdd(typeof (T), t => factory());
        }
        
    }
}
