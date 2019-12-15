using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using System.Collections.Immutable;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
	/// <summary>
	/// Default DotVVM control resolver.
	/// </summary>
	public abstract class ControlResolverBase : IControlResolver
	{
		private readonly DotvvmMarkupConfiguration configuration;

		private readonly ConcurrentDictionary<string, IControlType> cachedTagMappings = new ConcurrentDictionary<string, IControlType>(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<IControlType, IControlResolverMetadata> cachedMetadata = new ConcurrentDictionary<IControlType, IControlResolverMetadata>();

		private readonly Lazy<IControlResolverMetadata> htmlGenericControlMetadata;

		/// <summary>
		/// Initializes a new instance of the <see cref="ControlResolverBase"/> class.
		/// </summary>
		public ControlResolverBase(DotvvmMarkupConfiguration configuration)
		{
			this.configuration = configuration;
			foreach (var ccc in this.BindingTypes.Keys.ToArray())
			{
				BindingTypes[ccc] = BindingTypes[ccc].AddImports(configuration.ImportedNamespaces).AddParameters(configuration.DefaultExtensionParameters);
			}


			htmlGenericControlMetadata = new Lazy<IControlResolverMetadata>(() => ResolveControl(new ResolvedTypeDescriptor(typeof(HtmlGenericControl))));
		}

		/// <summary>
		/// Resolves the metadata for specified element.
		/// </summary>
		public virtual IControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters)
		{
			// html element has no prefix
			if (string.IsNullOrEmpty(tagPrefix))
			{
				activationParameters = new object[] { tagName };
				return htmlGenericControlMetadata.Value;
			}

			// find cached value
			var searchKey = GetSearchKey(tagPrefix, tagName);
			activationParameters = null;
			var controlType = cachedTagMappings.GetOrAdd(searchKey, _ => FindControlType(tagPrefix, tagName));
			if (controlType == null) return null;
			return ResolveControl(controlType);
		}

		private static string GetSearchKey(string tagPrefix, string tagName)
		{
			return tagPrefix + ":" + tagName;
		}

		/// <summary>
		/// Resolves the control metadata for specified type.
		/// </summary>
		public IControlResolverMetadata ResolveControl(IControlType controlType)
		{
			return cachedMetadata.GetOrAdd(controlType, _ => BuildControlMetadata(controlType));
		}

		/// <summary>
		/// Resolves the control metadata for specified type.
		/// </summary>
		public abstract IControlResolverMetadata ResolveControl(ITypeDescriptor controlType);



		public Dictionary<string, BindingParserOptions> BindingTypes = new Dictionary<string, BindingParserOptions>(StringComparer.OrdinalIgnoreCase)
		{
			{ ParserConstants.ValueBinding, BindingParserOptions.Create(typeof(ValueBindingExpression<>)) },
			{ ParserConstants.CommandBinding, BindingParserOptions.Create(typeof(CommandBindingExpression<>)) },
			{ ParserConstants.ControlPropertyBinding, BindingParserOptions.Create(typeof(ControlPropertyBindingExpression<>), "_control") },
			{ ParserConstants.ControlCommandBinding, BindingParserOptions.Create(typeof(ControlCommandBindingExpression<>), "_control") },
			{ ParserConstants.ResourceBinding, BindingParserOptions.Create(typeof(ResourceBindingExpression<>)) },
			{ ParserConstants.StaticCommandBinding, BindingParserOptions.Create(typeof(StaticCommandBindingExpression<>)) },
		};

		/// <summary>
		/// Resolves the binding type.
		/// </summary>
		public virtual BindingParserOptions ResolveBinding(string bindingType)
		{
			BindingParserOptions bpo;
			if (BindingTypes.TryGetValue(bindingType, out bpo))
			{
				return bpo;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Finds the control metadata.
		/// </summary>
		protected virtual IControlType FindControlType(string tagPrefix, string tagName)
		{
			// try to match the tag prefix and tag name
			var rules = configuration.Controls.Where(r => r.IsMatch(tagPrefix, tagName)).ToArray();
			// first try find markup control (see #155)
			foreach (var rule in rules)
			{
				rule.Validate();
				if (!string.IsNullOrEmpty(rule.TagName))
				{
					return FindMarkupControl(rule.Src.NotNull());
				}
			}
			// then code only control
			foreach (var rule in rules)
			{
				if (string.IsNullOrEmpty(rule.TagName))
				{
					var compiledControl = FindCompiledControl(tagName, rule.Namespace.NotNull(), rule.Assembly.NotNull());
					if (compiledControl != null)
					{
						return compiledControl;
					}
				}
			}
			return null;

			throw new Exception($"The control <{tagPrefix}:{tagName}> could not be resolved! Make sure that the tagPrefix is registered in DotvvmConfiguration.Markup.Controls collection!");
		}

        /// <summary>
        /// Finds the property in the control metadata.
        /// </summary>
        public IPropertyDescriptor FindProperty(IControlResolverMetadata controlMetadata, string name)
        {
            // try to find the property in metadata
            IPropertyDescriptor property;
            if (controlMetadata.TryGetProperty(name, out property))
            {
                return property;
            }

            // try to find an attached property
            if (name.Contains("."))
            {
                return FindGlobalProperty(name);
            }

            // try property group
            foreach (var group in controlMetadata.PropertyGroups)
            {
                if (name.StartsWith(group.Prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var concreteName = name.Substring(group.Prefix.Length);
                    return group.PropertyGroup.GetDotvvmProperty(concreteName);
                }
            }

            return null;
        }


        /// <summary>
        /// Finds the DotVVM property in the global property store.
        /// </summary>
        protected abstract IPropertyDescriptor FindGlobalProperty(string name);

        /// <summary>
        /// Finds the compiled control.
        /// </summary>
        protected abstract IControlType FindCompiledControl(string tagName, string namespaceName, string assemblyName);

		/// <summary>
		/// Finds the markup control.
		/// </summary>
		protected abstract IControlType FindMarkupControl(string file);

		/// <summary>
		/// Gets the control metadata.
		/// </summary>
		public abstract IControlResolverMetadata BuildControlMetadata(IControlType type);


	}
}
