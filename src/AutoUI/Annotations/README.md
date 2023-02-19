# Annotations for DotVVM Auto UI

This package only contains annotations and some interfaces used to annotate classes for which [DotVVM.AutoUI](https://www.nuget.org/packages/DotVVM.AutoUI) can create forms and tables.

DotVVM.AutoUI.Annotations only depends on DotVVM.Core, and is intended to be included in non-web projects.

Attributes included:
* `VisibleAttribute`, `EnabledAttribute`
* `ComboBoxSettingsAttribute`
* `StyleAttribute` for hardcoding css classes
* `SelectionAttribute` for declaring selectable fields using components like RadioButton

Base classes included:
* `Selection` for selectable items
