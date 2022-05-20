using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotVVM.AutoUI.Annotations;

namespace DotVVM.AutoUI.Metadata.Builder;

public class PropertyDisplayMetadataModifier
{
    private List<Action<PropertyDisplayMetadata>> actions = new();

    public PropertyDisplayMetadataModifier UseSelector<T>() where T : Annotations.Selection
    {
        actions.Add(m => m.SelectorConfiguration = new SelectorAttribute(typeof(T)));
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

    public PropertyDisplayMetadataModifier Ignore()
    {
        actions.Add(m => m.AutoGenerateField = false);
        return this;
    }

    public PropertyDisplayMetadataModifier ShowForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes = new[] { new VisibleAttribute() { ViewNames = viewNames } };
        });
        return this;
    }

    public PropertyDisplayMetadataModifier ShowForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes = new[] { new VisibleAttribute() { Roles = roles } };
        });
        return this;
    }

    public PropertyDisplayMetadataModifier ShowIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m =>
        {
            m.AutoGenerateField = true;
            m.VisibleAttributes = new[] { new VisibleAttribute() { IsAuthenticated = mode } };
        });
        return this;
    }

    public PropertyDisplayMetadataModifier SetDataType(DataType dataType)
    {
        actions.Add(m => m.DataType = dataType);
        return this;
    }

    public PropertyDisplayMetadataModifier Enable(bool allowEdit = true)
    {
        actions.Add(m => m.IsEditable = allowEdit);
        return this;
    }

    public PropertyDisplayMetadataModifier EnableForViews(string viewNames)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes = new[] { new EnabledAttribute() { ViewNames = viewNames } };
        });
        return this;
    }

    public PropertyDisplayMetadataModifier EnableForRoles(string roles)
    {
        actions.Add(m =>
        {
            m.IsEditable = true;
            m.EnabledAttributes = new[] { new EnabledAttribute() { Roles = roles } };
        });
        return this;
    }

    public PropertyDisplayMetadataModifier EnableIfAuthenticated(AuthenticationMode mode)
    {
        actions.Add(m => {
            m.IsEditable = true;
            m.EnabledAttributes = new[] { new EnabledAttribute() { IsAuthenticated = mode } };
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
