using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotVVM.Framework.Configuration
{
	public class ServiceLocator
	{
		//#if DotNetCore
		//		// wrapper around IServiceProvider
		//		private IServiceProvider sericeProvider;

		//		public void RegisterSingleton<T>(Func<T> factory)
		//		{
		//			factories[typeof(T)] = CreateSingletonFactory(factory);
		//			object temp;
		//			singletonInstances.TryRemove(typeof(T), out temp); //removes old service instance and allows to create new one
		//		}



		//		public T GetService<T>()
		//		{
		//			return sericeProvider.GetService<T>();
		//		}

		//		private Func<object> CreateSingletonFactory<T>(Func<T> factory)
		//		{
		//			return () => singletonInstances.GetOrAdd(typeof(T), t => factory());
		//		}
		//#else
		private ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();
		private ConcurrentDictionary<Type, object> singletonInstances = new ConcurrentDictionary<Type, object>();


		public void RegisterTransient<T>(Func<T> factory)
		{
			factories[typeof(T)] = () => factory();
		}

		public void RegisterSingleton<T>(Func<T> factory)
		{
			factories[typeof(T)] = CreateSingletonFactory(factory);
			object temp;
			singletonInstances.TryRemove(typeof(T), out temp); //removes old service instance and allows to create new one
		}



		public T GetService<T>()
		{
			Func<object> result;
			return factories.TryGetValue(typeof(T), out result) ? (T)result() : (T)((IServiceProvider)factories[typeof(IServiceProvider)]()).GetService(typeof(T));

		}

		private Func<object> CreateSingletonFactory<T>(Func<T> factory)
		{
			return () => singletonInstances.GetOrAdd(typeof(T), t => factory());
		}
		//#endif
	}
}
