using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotVVM.Framework.Configuration
{
	public class ServiceLocator
	{
		private readonly IServiceProvider serviceProvider;

		public ServiceLocator(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public T GetService<T>() => (T)serviceProvider.GetService(typeof(T));
	}
}
