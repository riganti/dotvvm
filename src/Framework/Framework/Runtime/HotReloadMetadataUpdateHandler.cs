using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
#if NET6_0_OR_GREATER
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(DotVVM.Framework.Runtime.HotReloadMetadataUpdateHandler))]
#endif
namespace DotVVM.Framework.Runtime
{
    public static class HotReloadMetadataUpdateHandler
    {
        internal static readonly ConcurrentBag<WeakReference<ViewModelSerializationMapper>> SerializationMappers = new();
        internal static readonly ConcurrentBag<WeakReference<UserColumnMappingCache>> UserColumnMappingCaches = new();
        internal static readonly ConcurrentBag<WeakReference<ExtensionMethodsCache>> ExtensionMethodsCaches = new();
        public static void ClearCache(Type[]? updatedTypes)
        {
            if (updatedTypes is null or { Length: 0})
                return;

            var problematicTypes = new HashSet<Type>();
            foreach (var sRef in SerializationMappers)
            {
                if (sRef.TryGetTarget(out var s))
                    foreach (var u in updatedTypes)
                    {
                        if (!s.ClearCache(u))
                            problematicTypes.Add(u);
                    }
            }
            foreach (var cRef in UserColumnMappingCaches)
            {
                if (cRef.TryGetTarget(out var c))
                    c.ClearCaches(updatedTypes);
            }
            foreach (var eRef in UserColumnMappingCaches)
            {
                if (eRef.TryGetTarget(out var e))
                    e.ClearCaches(updatedTypes);
            }

            ViewModelTypeMetadataSerializer.ClearCaches(updatedTypes);
            DefaultViewModelLoader.ClearCaches(updatedTypes);
            AttributeViewModelParameterBinder.ClearCaches(updatedTypes);
            ChildViewModelsCache.ClearCaches(updatedTypes);
            ActionFilterHelper.ClearCaches(updatedTypes);
            AttributeViewModelParameterBinder.ClearCaches(updatedTypes);
            DefaultControlUsageValidator.ClearCaches(updatedTypes);
            StyleMatcher.ClearCaches(updatedTypes);
            LifecycleRequirementsAssigningVisitor.ClearCaches(updatedTypes);
            ReflectionUtils.ClearCaches(updatedTypes);

            if (problematicTypes.Count > 0)
            {
                // yea, Console.WriteLine is the only way how to let the developer know AFAIK
                Console.WriteLine("DotVVM Hot Reload has a problem, we could not refresh metadata for these types: " + string.Join(", ", problematicTypes.Select(t => t.FullName)));
            }
        }
        // public static void UpdateApplication(Type[]? updatedTypes)
        // {
        //     if (updatedTypes is null or { Length: 0})
        //         return;
        // }
    }
}
