using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.DynamicData.Metadata.Builder;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.Configuration
{
    /// <summary>
    /// Represents the configuration of DotVVM Dynamic Data library.
    /// </summary>
    public class DynamicDataConfiguration
    {
        public Dictionary<Type, object> Properties { get; } = new Dictionary<Type, object>();

        /// <summary>
        /// Gets a list of registered providers that render form fields.
        /// </summary>
        public List<IFormEditorProvider> FormEditorProviders { get; private set; } = new List<IFormEditorProvider>();

        /// <summary>
        /// Gets a list of registered providers that render GridView columns.
        /// </summary>
        public List<IGridColumnProvider> GridColumnProviders { get; private set; } = new List<IGridColumnProvider>();

        /// <summary>
        /// Gets or sets whether the localization resource files for field display names and error messages will be used.
        /// </summary>
        public bool UseLocalizationResourceFiles { get; set; }

        /// <summary>
        /// Gets or sets the RESX file class with display names of the fields.
        /// </summary>
        public Type? PropertyDisplayNamesResourceFile { get; set; }

        /// <summary>
        /// Gets or sets the RESX file class with localized error messages.
        /// </summary>
        public Type? ErrorMessagesResourceFile { get; set; }

        /// <summary>
        /// Gets or sets the collection of rules applied on the auto-generated fields.
        /// </summary>
        public PropertyMetadataModifierCollection PropertyMetadataRules { get; set; }

        public DynamicDataConfiguration()
        {
            FormEditorProviders.Add(new SelectorComboBoxFormEditorProvider());
            FormEditorProviders.Add(new TextBoxEditorProvider());
            FormEditorProviders.Add(new CheckBoxEditorProvider());
            FormEditorProviders.Add(new EnumComboBoxFormEditorProvider());

            GridColumnProviders.Add(new CheckBoxGridColumnProvider());
            GridColumnProviders.Add(new TextGridColumnProvider());

            PropertyMetadataRules = new PropertyMetadataModifierCollection();
        }

        /// <summary>
        /// Browses the specified assembly and auto-registers all form editor providers.
        /// </summary>
        public void AutoDiscoverFormEditorProviders(Assembly assembly)
        {
            AutoDiscoverProviders(assembly, FormEditorProviders);
        }

        /// <summary>
        /// Browses the specified assembly and auto-registers all grid column providers.
        /// </summary>
        public void AutoDiscoverGridColumnProviders(Assembly assembly)
        {
            AutoDiscoverProviders(assembly, GridColumnProviders);
        }

        private void AutoDiscoverProviders<T>(Assembly assembly, IList<T> targetCollection)
        {
            var types = assembly.GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Where(t => !t.GetTypeInfo().IsAbstract)
                .Where(t => t.GetTypeInfo().DeclaredConstructors.Any(c => c.GetParameters().Length == 0));

            foreach (var type in types)
            {
                try
                {
                    var instance = Activator.CreateInstance(type).NotNull();
                    targetCollection.Insert(0, (T)instance);
                }
                catch (TargetInvocationException ex)
                {
                    throw new Exception($"Could not invoke the default constructor of {type.FullName}!", ex);
                }
            }
        }
    }
}
