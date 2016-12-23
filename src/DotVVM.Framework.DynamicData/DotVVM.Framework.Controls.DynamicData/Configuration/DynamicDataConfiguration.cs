using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.DynamicData.Builders;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns;

namespace DotVVM.Framework.Controls.DynamicData.Configuration
{
    public class DynamicDataConfiguration
    {

        public List<IFormEditorProvider> FormEditorProviders { get; private set; } = new List<IFormEditorProvider>();

        public List<IGridColumnProvider> GridColumnProviders { get; private set; } = new List<IGridColumnProvider>();

        public Dictionary<string, IFormBuilder> FormBuilders { get; private set; } = new Dictionary<string, IFormBuilder>()
        {
            { "", new TableDynamicFormBuilder() },
            { "bootstrap", new BootstrapFormGroupBuilder() }
        };
        

        public bool UseLocalizationResourceFiles { get; set; }

        public Type PropertyDisplayNamesResourceFile { get; set; }

        public Type ErrorMessagesResourceFile { get; set; }


        public DynamicDataConfiguration()
        {
            FormEditorProviders.Add(new CheckBoxEditorProvider());
            FormEditorProviders.Add(new TextBoxEditorProvider());

            GridColumnProviders.Add(new CheckBoxGridColumnProvider());
            GridColumnProviders.Add(new TextGridColumnProvider());
        }

        

        public IFormBuilder GetFormBuilder(string formBuilderName = "")
        {
            IFormBuilder builder;
            if (!FormBuilders.TryGetValue(formBuilderName, out builder))
            {
                throw new ArgumentException($"The {nameof(IFormBuilder)} with name '{formBuilderName}' was not found! Make sure it is registered in the {nameof(DynamicDataExtensions.ConfigureDynamicData)} method.");
            }
            return builder;
        }
    }
}
