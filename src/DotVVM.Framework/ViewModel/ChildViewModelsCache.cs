using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.ViewModel
{
    public static class ChildViewModelsCache
    {
        private static readonly ConcurrentDictionary<Type, Func<Type, PropertyInfo[]>> childViewModelsCollectionCache = new ConcurrentDictionary<Type, Func<Type, PropertyInfo[]>>();
        private static readonly ConcurrentDictionary<Type, Func<Type, PropertyInfo[]>> childViewModelsPropertiesCache = new ConcurrentDictionary<Type, Func<Type, PropertyInfo[]>>();

        public static PropertyInfo[] GetChildViewModelsCollection(Type viewModelType)
        {
            var childViewModelsPropertyInfoFactory = childViewModelsCollectionCache.GetOrAdd(viewModelType, type => GetChildViewModelsCollectionCore(type));
            return childViewModelsPropertyInfoFactory(viewModelType);
        }

        public static PropertyInfo[] GetChildViewModelsProperties(Type viewModelType)
        {
            var childViewModelsPropertyInfoFactory = childViewModelsPropertiesCache.GetOrAdd(viewModelType, type => GetChildViewModelsPropertiesCore(type));
            return childViewModelsPropertyInfoFactory(viewModelType);
        }
        private static PropertyInfo[] GetChildViewModelsCollectionCore(Type viewModelType)
        {
            var viewModels = viewModelType
                .GetProperties()
                .Where(p => typeof(IEnumerable<IDotvvmViewModel>).IsAssignableFrom(p.PropertyType)); ;

            return viewModels.ToArray();
        }

        //TODO Check collection
        private static PropertyInfo[] GetChildViewModelsPropertiesCore(Type viewModelType)
        {
            var viewModels = viewModelType
                .GetProperties()
                .Where(p => typeof(IDotvvmViewModel).IsAssignableFrom(p.PropertyType));;

            return viewModels.ToArray();
        }
    }
}