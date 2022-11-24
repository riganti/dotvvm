using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotVVM.AutoUI.Annotations;

namespace DotVVM.AutoUI.Metadata.Builder;

public class PropertyDisplayMetadataModifier
{
    private readonly List<Action<PropertyDisplayMetadata>> actions = new();

    /// <summary>
    /// Marks the property with SelectionAttribute indicating that the user will select a value from a list of TItem provided by ISelectionProvider&lt;TItem&gt;.
    /// </summary>
    public PropertyDisplayMetadataModifier SetSelection<TItem>() where TItem : Annotations.Selection
    {
        actions.Add(m => m.SelectionConfiguration = new SelectionAttribute(typeof(TItem)));
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.ResourceType and DisplayAttribute.Name properties to a resource type and key indicating that the display name of the property should be taken from the resource file.
    /// </summary>
    public PropertyDisplayMetadataModifier SetDisplayName(Type resourceName, string displayNameKey)
    {
        actions.Add(m => m.DisplayName = LocalizableString.Localized(resourceName, displayNameKey));
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.Name property to configure the display name of the property.
    /// </summary>
    public PropertyDisplayMetadataModifier SetDisplayName(string displayName)
    {
        actions.Add(m => m.DisplayName = LocalizableString.Constant(displayName));
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.GroupName property to configure the group in which the property belongs. The group name is an identifier which can be used to filter properties in auto-generated forms or table using the GroupName property.
    /// </summary>
    public PropertyDisplayMetadataModifier SetGroupName(string groupName)
    {
        actions.Add(m => m.GroupName = groupName);
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.Order property to configure the order of the property in the object.
    /// </summary>
    public PropertyDisplayMetadataModifier SetOrder(int? order)
    {
        actions.Add(m => m.Order = order);
        return this;
    }

    /// <summary>
    /// Sets the DisplayFormatAttribute.DataFormatString property to configure the format string for the property values.
    /// </summary>
    public PropertyDisplayMetadataModifier SetFormatString(string formatString)
    {
        actions.Add(m => m.FormatString = formatString);
        return this;
    }

    /// <summary>
    /// Sets the DisplayFormatAttribute.NullDisplayText property to configure the text representing the null value.
    /// </summary>
    public PropertyDisplayMetadataModifier SetNullDisplayText(string nullDisplayText)
    {
        actions.Add(m => m.NullDisplayText = nullDisplayText);
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.AutoGenerateField property to false indicating that the field should be hidden.
    /// </summary>
    public PropertyDisplayMetadataModifier Hide()
    {
        actions.Add(m => m.AutoGenerateField = false);
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.AutoGenerateField property indicating whether the field should be visible or hidden.
    /// </summary>
    public PropertyDisplayMetadataModifier SetAutoGenerateField(bool value)
    {
        actions.Add(m => m.AutoGenerateField = value);
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.AutoGenerateField property to true, and adds a VisibleAttribute to specify the views in which the field should be visible.
    /// The view name is an identifier which can be used to filter properties in auto-generated forms or table using the ViewName property.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Insert            // visible only in the Insert view
    /// - Insert | Edit     // visible in Insert or Edit views
    /// - !Insert &amp; !Edit   // visible in all views except for Insert and Edit
    /// </summary>
    public PropertyDisplayMetadataModifier ShowForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { ViewNames = viewNames });
        });
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.AutoGenerateField property to true, and adds a VisibleAttribute to specify the user roles for which the field should be visible.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Admin                   // visible only for the Admin role
    /// - Admin | Contributor     // visible for Admin or Contributor roles
    /// - !Admin &amp; !Contributor   // visible for all roles except for Admin or Contributor roles
    /// </summary>
    public PropertyDisplayMetadataModifier ShowForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { Roles = roles });
        });
        return this;
    }

    /// <summary>
    /// Sets the DisplayAttribute.AutoGenerateField property to true, and adds a VisibleAttribute to specify that the field should be visible only for authenticated users.
    /// </summary>
    public PropertyDisplayMetadataModifier ShowIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { IsAuthenticated = mode });
        });
        return this;
    }

    /// <summary>
    /// Sets the DataTypeAttribute.DataType property indicating the type of the value in the field.
    /// </summary>
    public PropertyDisplayMetadataModifier SetDataType(DataType dataType)
    {
        actions.Add(m => m.DataType = dataType);
        return this;
    }

    /// <summary>
    /// Sets the EditableAttribute.AllowEdit property indicating whether the field can be edited by the user.
    /// </summary>
    public PropertyDisplayMetadataModifier SetIsEditable(bool value = true)
    {
        actions.Add(m => m.IsEditable = value);
        return this;
    }

    /// <summary>
    /// Sets the EditableAttribute.AllowEdit property to true, and adds a EnabledAttribute to specify the views in which the field should be editable.
    /// The view name is an identifier which can be used to filter properties in auto-generated forms or table using the ViewName property.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Insert            // editable only in the Insert view
    /// - Insert | Edit     // editable in Insert or Edit views
    /// - !Insert &amp; !Edit   // editable in all views except for Insert and Edit
    /// </summary>
    public PropertyDisplayMetadataModifier EnableForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { ViewNames = viewNames });
        });
        return this;
    }

    /// <summary>
    /// Sets the EditableAttribute.AllowEdit property to true, and adds a EditableAttribute to specify the user roles for which the field should be visible.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Admin                   // editable only for the Admin role
    /// - Admin | Contributor     // editable for Admin or Contributor roles
    /// - !Admin &amp; !Contributor   // editable for all roles except for Admin or Contributor roles
    /// </summary>
    public PropertyDisplayMetadataModifier EnableForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { Roles = roles });
        });
        return this;
    }

    /// <summary>
    /// Sets the EditableAttribute.AllowEdit property to true, and adds a EditableAttribute to specify that the field should be visible only for authenticated users.
    /// </summary>
    public PropertyDisplayMetadataModifier EnableIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m => {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { IsAuthenticated = mode });
        });
        return this;
    }

    /// <summary>
    /// Sets whether the label for the field should be displayed.
    /// </summary>
    public PropertyDisplayMetadataModifier DisplayLabel(bool displayLabel = true)
    {
        actions.Add(m => m.IsDefaultLabelAllowed = displayLabel);
        return this;
    }

    /// <summary>
    /// Appends to the StyleAttribute.FormControlContainerCssClass indicating that a specified CSS class should be appended to the container of the form control.
    /// </summary>
    public PropertyDisplayMetadataModifier AddFormControlContainerCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlContainerCssClass += " " + cssClass);
        return this;
    }

    /// <summary>
    /// Appends to the StyleAttribute.FormRowCssClass indicating that a specified CSS class should be appended to the row in the form.
    /// </summary>
    public PropertyDisplayMetadataModifier AddFormRowCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormRowCssClass += " " + cssClass);
        return this;
    }

    /// <summary>
    /// Appends to the StyleAttribute.FormControlCssClass indicating that a specified CSS class should be appended to the editor control in the form.
    /// </summary>
    public PropertyDisplayMetadataModifier AddFormControlCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlCssClass += " " + cssClass);
        return this;
    }

    /// <summary>
    /// Appends to the StyleAttribute.GridCellCssClass indicating that a specified CSS class should be appended to the value cell in the grid.
    /// </summary>
    public PropertyDisplayMetadataModifier AddGridCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridCellCssClass += " " + cssClass);
        return this;
    }

    /// <summary>
    /// Appends to the StyleAttribute.GridHeaderCellCssClass indicating that a specified CSS class should be appended to the header cell in the grid.
    /// </summary>
    public PropertyDisplayMetadataModifier AddGridHeaderCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridHeaderCellCssClass += " " + cssClass);
        return this;
    }

    /// <summary>
    /// Sets a list of UI hint identifiers that help to select the best editor for the property. The first registered editor which contains at least one UI hint will be used.
    /// </summary>
    public PropertyDisplayMetadataModifier SetUIHint(params string[] hints)
    {
        actions.Add(m => m.UIHints = hints);
        return this;
    }

    /// <summary>
    /// Applies a custom action on PropertyDisplayMetadata object to configure the field.
    /// </summary>
    public PropertyDisplayMetadataModifier Configure(Action<PropertyDisplayMetadata> configurationAction)
    {
        actions.Add(configurationAction);
        return this;
    }

    internal void ApplyModifiers(PropertyDisplayMetadata metadata)
    {
        foreach (var action in actions)
        {
            action(metadata);
        }
    }
}
