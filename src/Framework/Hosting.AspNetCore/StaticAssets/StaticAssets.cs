#if NET9_0_OR_GREATER
#nullable enable
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.AspNetCore.StaticAssets;

public sealed class StaticAssetVirtualPathTranslator : IDotvvmVirtualPathTranslator
{
    private readonly FrozenDictionary<string, string> assetPathMap;
    public StaticAssetVirtualPathTranslator(StaticAssetsProvider provider)
    {
        assetPathMap = provider.GetAssetLabelMap()
            .ToDictionary(kv => "~/" + kv.Key, kv => "~/" + kv.Value.Route)
            .ToFrozenDictionary();
    }
    public string TranslateVirtualPath(string virtualUrl, IHttpContext httpContext)
    {
        if (!virtualUrl.StartsWith("~/", StringComparison.Ordinal))
            return virtualUrl;

        if (assetPathMap.TryGetValue(virtualUrl, out var mappedPath))
        {
            virtualUrl = mappedPath;
        }

        return DotvvmVirtualPathTranslator.TranslateVirtualPath(virtualUrl, httpContext);
    }
}

public sealed class StaticAssetResourceRepository: IDotvvmResourceRepository
{
    private Lazy<FrozenDictionary<string, IResource>> resources;

    public StaticAssetResourceRepository(
        DotvvmConfiguration config,
        StaticAssetsProvider provider,
        IWebHostEnvironment env
    )
    {
        resources = new Lazy<FrozenDictionary<string, IResource>>(() =>
        {
            var dict = new Dictionary<string, IResource>();
            foreach (var (label, asset) in provider.GetAssetLabelMap())
            {
                if (label is null) continue;

                var physicalPath = env.WebRootFileProvider.GetFileInfo(asset.AssetPath)?.PhysicalPath;
                if (physicalPath is null) continue;

                var location = new AssetResourceLocation("~/" + asset.Route, physicalPath);

                if (label.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    dict[label] = new ScriptModuleResource(location);
                }
                else if (label.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    dict[label] = new StylesheetResource(location);
                }
            }
            return dict.ToFrozenDictionary();
        });
    }
    public IResource? FindResource(string name)
    {
        if (resources.Value.TryGetValue(name, out var resource))
        {
            return resource;
        }
        return null;
    }

    sealed class AssetResourceLocation(string url, string physicalPath) : IResourceLocation, ILocalResourceLocation
    {
        public string GetUrl(IDotvvmRequestContext context, string name) => context.TranslateVirtualPath(url);

        public Stream LoadResource(IDotvvmRequestContext context) => File.OpenRead(physicalPath);
    }
}

public sealed class StaticAssetResourceLocation: IResourceLocation, ILocalResourceLocation
{
    private string? url;
    private string? physicalPath;
    private readonly string name;
    public StaticAssetResourceLocation(string name)
    {
        this.name = name;
    }

    [MemberNotNull(nameof(url), nameof(physicalPath))]
    private void Initialize(IDotvvmRequestContext context)
    {
        var provider = context.Services.GetRequiredService<StaticAssetsProvider>();
        var env = context.Services.GetRequiredService<IWebHostEnvironment>();
        var assetMap = provider.GetAssetLabelMap();
        if (!assetMap.TryGetValue(name, out var asset))
            throw new InvalidOperationException($"Static asset with label '{name}' not found.");
        var filePath = env.WebRootFileProvider.GetFileInfo(asset.AssetPath).PhysicalPath;

        // thread-safety: the above code should be deterministic -> it's OK if one thread writes the url and another writes the physicalPath
        Interlocked.CompareExchange(ref url, "~/" + asset.Route, null);
        Interlocked.CompareExchange(ref physicalPath, filePath, null);
    }

    public string GetUrl(IDotvvmRequestContext context, string name)
    {
        if (url is null)
            Initialize(context);
        return url;
    }

    public Stream LoadResource(IDotvvmRequestContext context)
    {
        if (physicalPath is null)
            Initialize(context);
        return File.OpenRead(physicalPath);
    }
}

public sealed class StaticAssetsProvider
{
    Lock initLock = new Lock();
    Func<IEnumerable<StaticAssetDescriptor>>? assetsLazy;
    IReadOnlyList<StaticAssetDescriptor>? staticAssets;
    FrozenDictionary<string, StaticAssetDescriptor>? assetLabelMap;
    public IReadOnlyList<StaticAssetDescriptor> GetAssets()
    {
        if (staticAssets is null) InitializeStaticAssets();
        return staticAssets;
    }

    public FrozenDictionary<string, StaticAssetDescriptor> GetAssetLabelMap()
    {
        if (assetLabelMap is null) InitializeStaticAssets();
        return assetLabelMap;
    }

    internal void SetStaticAssets(Func<IEnumerable<StaticAssetDescriptor>> assetsLazy)
    {
        ArgumentNullException.ThrowIfNull(assetsLazy);
        lock (initLock)
        {
            if (this.staticAssets is not null)
                throw new InvalidOperationException("Static assets have already been initialized.");
            this.assetsLazy = assetsLazy;
        }
    }

    [MemberNotNull(nameof(staticAssets), nameof(assetLabelMap))]
    internal void InitializeStaticAssets()
    {
        if (assetsLazy is null) ThrowNotInitialized();
        
        lock (initLock)
        {

            var assets = assetsLazy();
            var staticAssets = assets.ToImmutableArray();

            var dict = new Dictionary<string, StaticAssetDescriptor>();
            foreach (var asset in staticAssets)
            {
                string? label = null;
                foreach (var prop in asset.Properties)
                {
                    if (prop.Name == "label")
                        label = prop.Value;
                }

                if (label is {})
                {
                    dict[label] = asset;
                }
            }
            var assetLabelMap = dict.ToFrozenDictionary();

            Volatile.Write(ref this.staticAssets, staticAssets);
            Volatile.Write(ref this.assetLabelMap, assetLabelMap);
        }
    }

    [DoesNotReturn]
    private void ThrowNotInitialized()
    {
        throw new InvalidOperationException("Static assets have not been initialized.");
    }
}
#endif
