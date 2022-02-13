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


        public override DotvvmControl BuildForm(DynamicDataContext dynamicDataContext)
        {
            var entityPropertyListProvider = dynamicDataContext.Services.GetRequiredService<IEntityPropertyListProvider>();

            // create the table
            var table = InitializeTable(dynamicDataContext);
            
            // create the rows
            var properties = GetPropertiesToDisplay(dynamicDataContext, entityPropertyListProvider);
            foreach (var property in properties)
            {
                // find the editorProvider for cell
                var editorProvider = FindEditorProvider(property, dynamicDataContext);
                if (editorProvider == null) continue;

                // create the row
                HtmlGenericControl labelCell, editorCell;
                var row = InitializeTableRow(table, property, dynamicDataContext, out labelCell, out editorCell);
                
                // create the label
                InitializeControlLabel(row, labelCell, editorProvider, property, dynamicDataContext);
                
                // create the editorProvider
                InitializeControlEditor(row, editorCell, editorProvider, property, dynamicDataContext);

                // create the validator
                InitializeValidation(row, labelCell, editorCell, editorProvider, property, dynamicDataContext);
            }
            return table;
        }

        /// <summary>
        /// Initializes the validation on the row.
        /// </summary>
        protected virtual void InitializeValidation(HtmlGenericControl row, HtmlGenericControl labelCell, HtmlGenericControl editorCell, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (dynamicDataContext.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelCell.Attributes.Set("class", " dynamicdata-required");
            }

            if (editorProvider.CanValidate)
            {
                row.SetValue(Validator.ValueProperty, editorProvider.GetValidationValueBinding(property, dynamicDataContext));
            }
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
        protected virtual void InitializeControlLabel(HtmlGenericControl row, HtmlGenericControl labelCell, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (editorProvider.RenderDefaultLabel)
            {
                labelCell.Children.Add(new Literal(property.DisplayName));
            }
        }

        /// <summary>
        /// Creates the contents of the editor cell for the specified property.
        /// </summary>
        protected virtual void InitializeControlEditor(HtmlGenericControl row, HtmlGenericControl editorCell, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            editorCell.AppendChildren(editorProvider.CreateControl(property, dynamicDataContext));
        }

    }
}
