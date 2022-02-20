using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData
{
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
    public class DynamicEntity : DynamicEntityBase
    {
        public DynamicEntity(IServiceProvider services) : base(services)
        {
        }

        public string? LabelCellCssClass
        {
            get { return (string?)GetValue(LabelCellCssClassProperty); }
            set { SetValue(LabelCellCssClassProperty, value); }
        }
        public static readonly DotvvmProperty LabelCellCssClassProperty =
            DotvvmProperty.Register<string, DynamicEntity>(nameof(LabelCellCssClass));

        public string? EditorCellCssClass
        {
            get { return (string?)GetValue(EditorCellCssClassProperty); }
            set { SetValue(EditorCellCssClassProperty, value); }
        }
        public static readonly DotvvmProperty EditorCellCssClassProperty =
            DotvvmProperty.Register<string, DynamicEntity>(nameof(EditorCellCssClass));

        public DotvvmControl GetContents(FieldProps props)
        {
            var context = this.CreateDynamicDataContext();

            // create the table
            var table = InitializeTable(context);
            
            // create the rows
            foreach (var property in GetPropertiesToDisplay(context, props.FieldSelector))
            {
                if (this.TryGetFieldTemplate(property, props) is {} field)
                {
                    table.AppendChildren(field);
                    continue;
                }
                // create the row
                var row = InitializeTableRow(property, context, out var labelCell, out var editorCell);
                
                // create the label
                labelCell.AppendChildren(InitializeControlLabel(property, context, props));
                
                // create the editorProvider
                editorCell.AppendChildren(CreateEditor(property, context, props));

                // create the validator
                InitializeValidation(row, labelCell, property, context);

                table.Children.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Creates the table element for the form.
        /// </summary>
        protected virtual HtmlGenericControl InitializeTable(DynamicDataContext dynamicDataContext) =>
            new HtmlGenericControl("table")
                .AddCssClass("dotvvm-dynamicdata-form-table");


        /// <summary>
        /// Creates the table row for the specified property.
        /// </summary>
        protected virtual HtmlGenericControl InitializeTableRow(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelCell, out HtmlGenericControl editorCell)
        {
            labelCell = new HtmlGenericControl("td")
                .AddCssClasses("dynamicdata-label", LabelCellCssClass);

            editorCell = new HtmlGenericControl("td")
                .AddCssClasses("dynamicdata-editor", EditorCellCssClass, property.Styles?.FormControlContainerCssClass);
            
            return new HtmlGenericControl("tr")
                .AddCssClass(property.Styles?.FormRowCssClass)
                .SetProperty(c => c.IncludeInPage, GetVisibleResourceBinding(property, dynamicDataContext))
                .AppendChildren(labelCell, editorCell);
        }
    }
}
