using System;

namespace DotVVM.Framework.ResourceManagement
{
    public static class ResourceRepositoryExtensions
    {
        /// <summary> Registers a <see cref="StylesheetResource" /> (a CSS resource) with the specified name, location and dependencies.
        /// <paramref name="integrityHash"/> does not have to be used when location is local. </summary>
        public static StylesheetResource RegisterStylesheet(
            this DotvvmResourceRepository repo,
            string name,
            IResourceLocation location,
            string[]? dependencies = null,
            string? integrityHash = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto)
        {
            var r = new StylesheetResource(location) {
                Dependencies = dependencies ?? Array.Empty<string>(),
                IntegrityHash = integrityHash,
                FetchPriority = fetchPriority
            };
            repo.Register(name, r);
            return r;
        }

        /// <summary> Registers a <see cref="StylesheetResource" /> from the specified file.
        /// The file can be anywhere in the filesystem, it does not have to be in the wwwroot folder.
        /// DotVVM will handle its serving, caching, ... automatically </summary>
        public static StylesheetResource RegisterStylesheetFile(
            this DotvvmResourceRepository repo,
            string name,
            string filePath,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterStylesheet(name, new FileResourceLocation(filePath), dependencies, null, fetchPriority);

        /// <summary> Registers a <see cref="StylesheetResource" /> with the specified URL.
        /// If the URL is local, consider using the <see cref="RegisterStylesheetFile(DotvvmResourceRepository, string, string, string[], ResourceFetchPriority)" /> method. </summary>
        /// <param name="integrityHash"> is a hash of the served file, it's highly recommended to set it when the resource is from a 3rd party domain. See https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity for more information. </param>
        public static StylesheetResource RegisterStylesheetUrl(
            this DotvvmResourceRepository repo,
            string name,
            string url,
            string? integrityHash,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterStylesheet(name, new UrlResourceLocation(url), dependencies, integrityHash, fetchPriority);

        /// <summary> Registers a <see cref="ScriptResource" /> (a Javascript resource) with the specified name, location and dependencies.
        /// <paramref name="integrityHash"/> does not have to be used when location is local. </summary>
        public static LinkResourceBase RegisterScript(
            this DotvvmResourceRepository repo,
            string name,
            IResourceLocation location,
            bool defer = true,
            bool module = false,
            string[]? dependencies = null,
            string? integrityHash = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto)
        {
            if (!defer && module)   
                throw new ArgumentException("<script type='module'> always deferred, please do not specify defer: false", nameof(defer));
            LinkResourceBase r = module ? new ScriptModuleResource(location) : new ScriptResource(location, defer);
            r.Dependencies = dependencies ?? Array.Empty<string>();
            r.IntegrityHash = integrityHash;
            r.FetchPriority = fetchPriority;
            repo.Register(name, r);
            return r;
        }

        /// <summary> Registers a <see cref="ScriptResource" /> from the specified file.
        /// The file can be anywhere in the filesystem, it does not have to be in the wwwroot folder.
        /// DotVVM will handle its serving, caching, ... automatically </summary>
        public static LinkResourceBase RegisterScriptFile(
            this DotvvmResourceRepository repo,
            string name,
            string filePath,
            bool defer = true,
            bool module = false,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterScript(name, new FileResourceLocation(filePath), defer, module, dependencies, null, fetchPriority);

        /// <summary> Registers a <see cref="ScriptModuleResource" /> from the specified file.
        /// The file can be anywhere in the filesystem, it does not have to be in the wwwroot folder.
        /// DotVVM will handle its serving, caching, ... automatically </summary>
        public static LinkResourceBase RegisterScriptModuleFile(
            this DotvvmResourceRepository repo,
            string name,
            string filePath,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterScript(name, new FileResourceLocation(filePath), defer: true, module: true, dependencies, fetchPriority: fetchPriority);

        /// <summary> Registers a <see cref="ScriptModuleResource" /> from the specified file.
        /// The file can be anywhere in the filesystem, it does not have to be in the wwwroot folder.
        /// DotVVM will handle its serving, caching, ... automatically </summary>
        [Obsolete("<script type='module'> is always deferred, the attribute does nothing. Please remove the defer parameter.")]
        public static LinkResourceBase RegisterScriptModuleFile(
            this DotvvmResourceRepository repo,
            string name,
            string filePath,
            bool defer,
            string[]? dependencies = null) =>
            repo.RegisterScript(name, new FileResourceLocation(filePath), defer: true, module: true, dependencies);

        /// <summary> Registers a <see cref="ScriptResource" /> with the specified URL.
        /// If the URL is local, consider using the <see cref="RegisterScriptFile(DotvvmResourceRepository, string, string, bool, bool, string[], ResourceFetchPriority)" /> method. </summary>
        /// <param name="integrityHash"> is a hash of the served file, it's highly recommended to set it when the resource is from a 3rd party domain. See https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity for more information. </param>
        public static LinkResourceBase RegisterScriptUrl(
            this DotvvmResourceRepository repo,
            string name,
            string url,
            string? integrityHash,
            bool defer = true,
            bool module = false,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterScript(name, new UrlResourceLocation(url), defer, module, dependencies, integrityHash, fetchPriority);

        /// <summary> Registers a <see cref="ScriptModuleResource" /> with the specified URL.
        /// If the URL is local, consider using the <see cref="RegisterScriptFile(DotvvmResourceRepository, string, string, bool, bool, string[], ResourceFetchPriority)" /> method. </summary>
        /// <param name="integrityHash"> is a hash of the served file, it's highly recommended to set it when the resource is from a 3rd party domain. See https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity for more information. </param>
        public static LinkResourceBase RegisterScriptModuleUrl(
            this DotvvmResourceRepository repo,
            string name,
            string url,
            string? integrityHash,
            bool defer = true,
            string[]? dependencies = null,
            ResourceFetchPriority fetchPriority = ResourceFetchPriority.Auto) =>
            repo.RegisterScript(name, new UrlResourceLocation(url), defer, module: true, dependencies, integrityHash, fetchPriority);
    }
}
