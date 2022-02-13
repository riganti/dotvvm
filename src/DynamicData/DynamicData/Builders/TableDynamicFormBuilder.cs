using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;

namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    /// <summary>
    /// Builds the dynamic form using a HTML table - the first column contain the field labels, the second column contains the form fields.
    /// </summary>
    public class TableDynamicFormBuilder : FormBuilderBase
    {

        public string LabelCellCssClass { get; set; }

        public string EditorCellCssClass { get; set; }


        public override DotvvmControl BuildForm(DynamicDataContext ddContext)
        {
            var entityPropertyListProvider = ddContext.Services.GetRequiredService<IEntityPropertyListProvider>();

            // create the table
            var table = InitializeTable(ddContext);
            
            // create the rows
            var properties = GetPropertiesToDisplay(ddContext, entityPropertyListProvider);
            foreach (var property in properties)
            {
                // create the row
                HtmlGenericControl labelCell, editorCell;
                var row = InitializeTableRow(table, property, ddContext, out labelCell, out editorCell);
                
                // create the label
                labelCell.AppendChildren(InitializeControlLabel(property, ddContext));
                
                // create the editorProvider
                editorCell.AppendChildren(InitializeControlEditor(property, ddContext));

                // create the validator
                InitializeValidation(row, labelCell, property, ddContext);
            }
            return table;
        }

        /// <summary>
        /// Initializes the validation on the row.
        /// </summary>
        protected virtual void InitializeValidation(HtmlGenericControl row, HtmlGenericControl labelCell, PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            if (ddContext.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelCell.Attributes.Set("class", " dynamicdata-required");
            }

            row.SetValue(Validator.ValueProperty, ddContext.CreateValueBinding(property.PropertyInfo.Name));
        }

        /// <summary>
        /// Creates the table element for the form.
        /// </summary>
        protected virtual HtmlGenericControl InitializeTable(DynamicDataContext dynamicDataContext)
        {
            var table = new HtmlGenericControl("table");
            table.Attributes.Set("class", "dotvvm-dynamicdata-form-table");

            return table;
        }

        /// <summary>
        /// Creates the table row for the specified property.
        /// </summary>
        protected virtual HtmlGenericControl InitializeTableRow(HtmlGenericControl table, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelCell, out HtmlGenericControl editorCell)
        {
            var row = new HtmlGenericControl("tr");
            row.Attributes.Set("class", property.Styles?.FormRowCssClass);
            table.Children.Add(row);

            labelCell = new HtmlGenericControl("td");
            labelCell.Attributes.Set("class", ControlHelpers.ConcatCssClasses("dynamicdata-label", LabelCellCssClass));
            row.Children.Add(labelCell);

            editorCell = new HtmlGenericControl("td");
            editorCell.Attributes.Set("class", ControlHelpers.ConcatCssClasses("dynamicdata-editor", EditorCellCssClass, property.Styles?.FormControlContainerCssClass));
            row.Children.Add(editorCell);
            
            return row;
        }

        /// <summary>
        /// Creates the contents of the label cell for the specified property.
        /// </summary>
        protected virtual DotvvmControl? InitializeControlLabel(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (property.IsDefaultLabelAllowed)
            {
                return new Literal(property.DisplayName ?? property.PropertyInfo.Name);
            }
            return null;
        }

        /// <summary>
        /// Creates the contents of the editor cell for the specified property.
        /// </summary>
        protected virtual DotvvmControl InitializeControlEditor(PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            return new DynamicEditor(ddContext.Services)
                .SetProperty(DynamicEditor.PropertyProperty, ddContext.CreateValueBinding(property.PropertyInfo.Name));
        }

    }
}
