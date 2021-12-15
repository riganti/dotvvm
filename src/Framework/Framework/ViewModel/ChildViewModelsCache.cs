using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;

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
                .Where(p => typeof(IEnumerable<IDotvvmViewModel>).IsAssignableFrom(p.PropertyType));

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

        public static ParametrizedCode RootViewModelPath = new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter).FormatParametrizedScript();

        static ConditionalWeakTable<IDotvvmViewModel, ParametrizedCode> viewModelPaths = new ConditionalWeakTable<IDotvvmViewModel, ParametrizedCode>();
        public static void SetViewModelClientPath(IDotvvmViewModel viewModel, ParametrizedCode path) =>
            viewModelPaths.Add(viewModel, path);

        public static ParametrizedCode GetViewModelClientPath(IDotvvmViewModel viewModel) =>
            viewModelPaths.TryGetValue(viewModel, out var p) ? p : p;

        /// <summary> Clear cache when hot reload happens </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
            {
                childViewModelsCollectionCache.TryRemove(t, out _);
                childViewModelsPropertiesCache.TryRemove(t, out _);
            }
        }
    }
}
