# DotVVM Dynamic Data

Reimplementation of **ASP.NET Dynamic Data** for [DotVVM](https://github.com/riganti/dotvvm).


## Data Annotations

The main goal of this library is to generate user interface from metadata. Currently, there are two interfaces:

* `IPropertyDisplayMetadataProvider` provides basic information about properties - the display name, format string, order, group name (you can split the fields into multiple groups and render each group separately).

* `IViewModelValidationMetadataProvider` allows to retrieve all validation attributes for each property.


## Initialization

First, install the `DotVVM.DynamicData` NuGet package in your project.

```
Install-Package DotVVM.DynamicData
```

To use Dynamic Data, add the following line to the `Startup.cs` file.

```
// ASP.NET Core (place this snippet in the ConfigureServices method)
services.AddDotVVM(options =>
{
    var dynamicDataConfig = new DynamicDataConfiguration();
    // set up config
    options.AddDynamicData(dynamicDataConfig);
});

// OWIN
app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, options: options =>
{
    var dynamicDataConfig = new DynamicDataConfiguration();
    // set up config
    options.AddDynamicData(dynamicDataConfig);
});
```

This will allow to provide UI metadata using the standard .NET Data Annotations attributes.

```
public class EmployeeDTO
{
    [Display(AutoGenerateField = false)]        // this field will be hidden
    public int Id { get; set; }



    // first group of fields

    [Required]
    [EmailAddress]
    [Display(Name = "User Name", Order = 1, GroupName = "Basic Info")]
    public string UserName { get; set; }

    [Required]
    [Display(Name = "First Name", Order = 2, GroupName = "Basic Info")]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name", Order = 3, GroupName = "Basic Info")]
    public string LastName { get; set; }

    [DisplayFormat(DataFormatString = "d")]
    [Display(Name = "Birth Date", Order = 4, GroupName = "Basic Info")]
    public DateTime BirthDate { get; set; }


    // second group of fields

    [Display(Name = "E-mail", Order = 11, GroupName = "Contact Info")]
    public string PersonalEmail { get; set; }

    [Display(Name = "Phone", Order = 12, GroupName = "Contact Info")]
    public string PersonalPhone { get; set; }

}
```

<br />

## GridView

Now, when you have your DTO class decorated with data annotation attributes, you can auto-generate GridView columns.

DotVVM Dynamic Data brings the `DynamicDataGridViewDecorator` control. Use this decorator on `GridView` to initialize the `Columns` collection.

```
    <dd:DynamicDataGridViewDecorator>
        <bs:GridView Type="Bordered" DataSource="{value: Employees}" />
    </dd:DynamicDataGridViewDecorator>
```

If you want to add your own columns (e.g. the edit button) to the auto-generated ones, you can use the `ColumnPlacement` to specify, whether
the generated columns should appear on the left side or the right side from your own columns.

```
    <dd:DynamicDataGridViewDecorator ColumnPlacement="Left">
        <bs:GridView Type="Bordered" DataSource="{value: Employees}">
            <Columns>

                <!-- The auto-generated columns will appear here because ColumnPlacement is Left. -->

                <dot:GridViewTemplateColumn>   <!-- your own column -->
                    <dot:LinkButton Click="{command: _parent.Edit(Id)}">
                        <bs:GlyphIcon Icon="Pencil" />
                    </dot:LinkButton>
                </dot:GridViewTemplateColumn>
            </Columns>
        </bs:GridView>
    </dd:DynamicDataGridViewDecorator>
```

<br />

## Generating Forms

DotVVM Dynamic Data also contains the `DynamicEntity` control - you can use it to generate forms.

```
<dd:DynamicEntity DataContext="{value: EditedEmployee}" />
```

The control takes its `DataContext` and generates form fields for all properties of the object using the metadata from data annotation attributes.

If you want the form to have a custom layout, you need to use the group names and render each group separately. If you specify the `GroupName` property, the `DynamicEntity` will render
only fields from this group.

```
<!-- This will render two columns. -->
<div class="row">
    <div class="col-md-6">
        <dd:DynamicEntity DataContext="{value: EditedEmployee}" GroupName="Basic Info" />
    </div>
    <div class="col-md-6">
        <dd:DynamicEntity DataContext="{value: EditedEmployee}" GroupName="Contact Info" />
    </div>
</div>
```

By default, the form is rendered using the [TableDynamicFormBuilder](./src/DotVVM.Framework.DynamicData/DotVVM.Framework.Controls.DynamicData/Builders/TableDynamicFormBuilder.cs) class.
This class renders HTML table with rows for each of the form fields.

You can write your own form builder and register it in the `DotvvmStartup.cs` class. The builder must implement the `IFormBuilder` interface.

```
config.ServiceLocator.RegisterSingleton<IFormBuilder>(() => new YourOwnFormBuilder());
```

If you have implemented your own form builder and there is a chance that it might be useful for other people, please send us PR and we'll be happy to include as part of the library.

<br />

## Custom Editors

Currently, the framework supports `TextBox` and `CheckBox` editors, which can edit string, numeric, date-time and boolean values.
If you want to support any other data type, you can implement your own editor and grid column.

You need to derive from the [FormEditorProviderBase](./src/DotVVM.Framework.DynamicData/DotVVM.Framework.Controls.DynamicData/PropertyHandlers/FormEditors/FormEditorProviderBase.cs) to implement a custom editor
in the form, and to derive from the [GridColumnProviderBase](./src/DotVVM.Framework.DynamicData/DotVVM.Framework.Controls.DynamicData/PropertyHandlers/GridColumns/GridColumnProviderBase.cs) to implement about
custom GridView column.

Then, you have to register the editor in the `DotvvmStartup.cs` file. Please note that the order of editor providers and grid columns matters. The Dynamic Data will use the first provider which returns `CanHandleProperty = true`
for the property.

```
dynamicDataConfig.FormEditorProviders.Add(new YourEditorProvider());
dynamicDataConfig.GridColumnProviders.Add(new YourGridColumnProvider());
```

<br />

## Loading Metadata from Resource Files

Decorating every field with the `[Display(Name = "Whatever")]` is not very effective when it comes to localization - you need to specify the resource file type and resource key.
Also, if you have multiple entities with the `FirstName` property, you'll probably want to use the same field name for all of them.

That's why DotVVM Dynamic Data comes with the resource-based metadata providers. They can be registered in the `DotvvmStartup.cs` like this:

```
config.RegisterResourceMetadataProvider(typeof(Resources.ErrorMessages), typeof(Resources.PropertyDisplayNames));
```

The `ErrorMessages` and `PropertyDisplayNames` are RESX files in the `Resources` folder and they contain the default error messages and display names of the properties.

### Localizing Error Messages

If you use the `[Required]` attribute and you don't specify the `ErrorMessage` or `ErrorMessageResourceName` on it, the resource provider will look in the `ErrorMessages.resx` file
and if it finds the `Required` key there, it'll use this resource item to provide the error message.

Your `ErrorMessages.resx` file may look like this:

```
Resource Key            Value
-------------------------------------------------------------------------------------
Required                {0} is required!
EmailAddress            {0} is not a valid e-mail address!
...
```

### Localizing Property Display Names

The second resource file `PropertyDisplayNames.resx` contains the display names. If the property doesn't have the `[Display(Name = "Something")]` attribute, the provider will look in the
resource file for the following values (in this order). If it finds an item with that key, it'll use the value as a display name of the field

* `TypeName_PropertyName`
* `PropertyName`

So if you want to use the text "Given Name" for the `FirstName` property in all classes, with the exception of the `ManagerDTO` class where you need to use the "First Name" text, your resource file
should look like this:

```
Resource Key            Value
-------------------------------------------------------------------------------------
FirstName               Given Name
ManagerDTO_FirstName    First Name
...
```

<br />

## Roadmap

Here is a brief list of features that are already done, and features that are planned for the future releases.

### Implemented

* `DisplayAttribute` (`Name`, `Order`, `GroupName`, `AutoGenerateField`)
* `DisplayFormatAttribute` (`DataFormatString`)
* Validation Attributes
* Resource lookup for validation error messages and property display names
* HTML table layout for Forms
* TextBox and CheckBox editors
* ComboBox editor with support of conventions

### In Progress

* `DisplayFormatAttribute` (`NullDisplayText`)
* `DynamicEditor` control for editing individual field
* DateTimePicker and UserControl editor
* More form layouts

### Future

* `UIHint` attribute support
* Auto-generating filters on top of the GridView
* Entity Relationship support
* Collection editors
* Page templates
