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

        public IFormBuilder FormBuilder { get; set; } = new TableDynamicFormBuilder();


        public DynamicDataConfiguration()
        {
            FormEditorProviders.Add(new CheckBoxEditorProvider());
            FormEditorProviders.Add(new TextBoxEditorProvider());

            GridColumnProviders.Add(new CheckBoxGridColumnProvider());
            GridColumnProviders.Add(new TextGridColumnProvider());
        }

    }
}
