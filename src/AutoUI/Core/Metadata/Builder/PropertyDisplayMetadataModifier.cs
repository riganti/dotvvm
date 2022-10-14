using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotVVM.AutoUI.Annotations;

namespace DotVVM.AutoUI.Metadata.Builder;

public class PropertyDisplayMetadataModifier
{
    private readonly List<Action<PropertyDisplayMetadata>> actions = new();

    public PropertyDisplayMetadataModifier SetSelection<T>() where T : Annotations.Selection
    {
        actions.Add(m => m.SelectionConfiguration = new SelectionAttribute(typeof(T)));
        return this;
    }

    public PropertyDisplayMetadataModifier SetDisplayName(Type resourceName, string displayNameKey)
    {
        actions.Add(m => m.DisplayName = LocalizableString.Localized(resourceName, displayNameKey));
        return this;
    }

    public PropertyDisplayMetadataModifier SetDisplayName(string displayName)
    {
        actions.Add(m => m.DisplayName = LocalizableString.Constant(displayName));
        return this;
    }

    public PropertyDisplayMetadataModifier SetGroupName(string groupName)
    {
        actions.Add(m => m.GroupName = groupName);
        return this;
    }

    public PropertyDisplayMetadataModifier SetOrder(int? order)
    {
        actions.Add(m => m.Order = order);
        return this;
    }

    public PropertyDisplayMetadataModifier SetFormatString(string formatString)
    {
        actions.Add(m => m.FormatString = formatString);
        return this;
    }

    public PropertyDisplayMetadataModifier SetNullDisplayText(string nullDisplayText)
    {
        actions.Add(m => m.NullDisplayText = nullDisplayText);
        return this;
    }

    public PropertyDisplayMetadataModifier Hide()
    {
        actions.Add(m => m.AutoGenerateField = false);
        return this;
    }

    public PropertyDisplayMetadataModifier SetAutoGenerateField(bool value)
    {
        actions.Add(m => m.AutoGenerateField = value);
        return this;
    }

    public PropertyDisplayMetadataModifier ShowForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { ViewNames = viewNames });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier ShowForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { Roles = roles });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier ShowIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes.Add(new VisibleAttribute() { IsAuthenticated = mode });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier SetDataType(DataType dataType)
    {
        actions.Add(m => m.DataType = dataType);
        return this;
    }

    public PropertyDisplayMetadataModifier SetIsEditable(bool value = true)
    {
        actions.Add(m => m.IsEditable = value);
        return this;
    }

    public PropertyDisplayMetadataModifier EnableForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { ViewNames = viewNames });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier EnableForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { Roles = roles });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier EnableIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m => {
            m.IsEditable = true;
            m.EnabledAttributes.Add(new EnabledAttribute() { IsAuthenticated = mode });
        });
        return this;
    }

    public PropertyDisplayMetadataModifier DisplayLabel(bool displayLabel = true)
    {
        actions.Add(m => m.IsDefaultLabelAllowed = displayLabel);
        return this;
    }

    public PropertyDisplayMetadataModifier AddFormControlContainerCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlContainerCssClass += " " + cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetFormControlContainerCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlContainerCssClass = cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier AddFormRowCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormRowCssClass += " " + cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetFormRowCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormRowCssClass = cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier AddFormControlCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlCssClass += " " + cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetFormControlCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).FormControlCssClass = cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier AddGridCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridCellCssClass += " " + cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetGridCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridCellCssClass = cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier AddGridHeaderCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridHeaderCellCssClass += " " + cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetGridHeaderCellCssClass(string cssClass)
    {
        actions.Add(m => (m.Styles = (m.Styles ?? new StyleAttribute())).GridHeaderCellCssClass = cssClass);
        return this;
    }

    public PropertyDisplayMetadataModifier SetUIHint(params string[] hints)
    {
        actions.Add(m => m.UIHints = hints);
        return this;
    }

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
