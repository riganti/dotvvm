# DotVVM Auto UI

Automatically generated forms, tables and more from type metadata. 

## Data Annotations

The main goal of this library is to generate user interface from metadata.
It should be able to create reasonable UI from just the type information, for more control control over it you can use the following attributes.

* `[Display(Name = "X")]` - sets the property display name
* `[Display(Prompt = "enter your email")]` - sets the [placeholder](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input#placeholder) text of a textbox
* `[Display(Description = "Longer description of the data collected")]` - sets longer description. It will be set into the `title` attribute, and shown as sub-heading in some forms
* `[Display(Order = 10)]` - by default, the properties are in the order as they are defined in C#. This attribute allows you to change the order
* `[Display(GroupName = "A")]` - makes this field visible in form with `GroupName` property set to `A`
* `[Display(AutoGenerateField = false)]` - hide the field from forms. Note that a malicious user might still be able to see it and change it in the viewmodel, unless you also apply `[Protect(ProtectMode.EncryptData)]`, `[Bind(Direction.None)]` or `[JsonIgnore]`
* `[DisplayFormat(DataFormatString = "dd. MM. yyyy")]` - controls the format string used in textbox and literals
* `[DataType(...)]` - controls which input type will be generated for the specified property
    - `DataType.MultilineText` - `<textarea>`
    - `DataType.Password` - `<input type=password>`
    - `DataType.Date` - `<input type=date>`. Note that this works for both `DateTime` and `string` properties
    - `DataType.Time` - `<input type=time>`
    - `DataType.DateTime` - `<input type=datetime-local>`
    - You can add support for more by implementing the `IFormEditorProvider` interface
* `[Style(...)]` - allows adding additional CSS classes to different parts of the forms, or tables
* `[Editable(AllowEdit = false)]` - makes the read only. Note that without `[Protect(ProtectMode.Sign]` anyone can change the underlying viewmodel anyway.
* `[Visible(Roles = "Developer & !Manger")]` - makes the field visible only to authenticated users with the `Developer` roles and without the `Manager` role
* `[Visible(ViewNames = "Insert | Edit")]` - makes the field visible in forms with ViewName property set to `Insert` or `Edit`
* `[Enabled(...)]` - makes the field editable under some conditions (similar API to `[Visible(...)]`)
* `[UIHint(...)]` - currently not used by AutoUI, but can be used for matching custom providers

### Example

```csharp

```csharp
public class EmployeeDTO
{
    [Display(AutoGenerateField = false)]        // this field will be hidden
    public int Id { get; set; }

    [Required]
    [Display(GroupName = "Basic Info")]
    public string UserName { get; set; }

    [Required]
    [Display(GroupName = "Basic Info")]
    public string FirstName { get; set; }

    [Required]
    [Display(GroupName = "Basic Info")]
    public string LastName { get; set; }

    [Display(GroupName = "Basic Info")]
    public DateTime BirthDate { get; set; }


    [Display(Name = "E-mail", GroupName = "Contact Info")]
    [EmailAddress] // use <input type=email>
    public string PersonalEmail { get; set; }

    [Display(Name = "Phone", GroupName = "Contact Info")]
    [DataType(DataType.PhoneNumber)] // use <input type=tel>
    public string PersonalPhone { get; set; }

}

```

### Configuration API


The metadata can be also controlled using a configuration API:

```csharp

services.AddAutoUI(config => {
    // for all properties with a certain name
    config.PropertyMetadataRules
        .For("IsCompany", r => r.SetDisplayName(""))
        .For("ProductId", r => r.UseSelection<ProductSelection>());
})

```

The metadata provider can be easily extended by implementing and registering these two interfaces:

* `IPropertyDisplayMetadataProvider` provides basic information about properties - all information listed above.

* `IViewModelValidationMetadataProvider` retrieves validation attributes for each property.


## Initialization

First, install the `DotVVM.AutoUI` NuGet package in your project.

```
dotnet add package DotVVM.AutoUI
```

To use AutoUI, add the following line to the `Startup.cs` file.

```csharp
// ASP.NET Core (place this snippet in the ConfigureServices method)
services.AddDotVVM(options =>
{
    options.AddAutoUI(config => {
        // configuration options
    });
});

// OWIN
app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, options: options =>
{    // set up config
    options.AddAutoUI(config => {
        // configuration options
    });
});
```

This will allow to provide UI metadata using the standard .NET Data Annotations attributes.

<br />

## `GridView` - data tables

When your view model class is decorated with data annotation attributes, you can auto-generate GridView columns.

DotVVM AutoUI brings the `auto:GridViewColumns` control, it is a special grid column which get's replaced by a separate column for each property.
It can be used with the built-in `dot:GridView`, and also with the `GridView`s in DotVVM component packages

```html
<bs:GridView Type="Bordered" DataSource="{value: Employees}">
    <auto:GridViewColumns />
    <!-- It can be combined with any custom columns -->
    <dot:GridViewTemplateColumn>
        <a href='{value: $"/people/edit/{_this.Id}"}'> <bs:GlyphIcon Icon="Pencil" /> Edit</a>
    </dot:GridViewTemplateColumn>
</bs:GridView>
```

Number of properties can be used to customize the `auto:GridViewColumns` behavior. Most notably:

* `Property-LastAccessTime={value: _this.LastContactTime.ToBrowserLocalTime()}`
    - passes the LastContactTime property through the [`ToBrowserLocalTime`](https://www.dotvvm.com/Docs/ListItem/pages/concepts/localization-and-cultures/local-vs-utc-dates#converting-utc-time-to-the-browser-timezone) function
* `Property-Name={value: FirstName + " " + LastName}`
    - adds new column with the specified value binding
* `Header-LastContactTime="Last Seen"`
    - sets custom display name for the column
* `ExcludeProperties="FirstName, LastName"`
    - removes the FirstName and LastName columns from the table
* `IncludeProperties="Name, Email"`
    - only the listed properties will be included in the table
* `<ContentTemplate-Id> <a href='{value: $"/people/detail/{Id}"}'> {{value: Id}} </a> </ContentTemplate-Id>`
    - sets a custom HTML template for the `Id` column 
* `IsEditable-Id=false`
    - marks the `Id` property as not editable (only in this GridView's inline edit mode)
* `<EditorTemplate-MyProperty> ...` - similar to `ContentTemplate-X`

There is also similar `auto:GridViewColumn` which is used for a single property. 

## Forms

DotVVM AutoUI contains the `auto:Form` control for basic forms.
We also include `auto:BootstrapForm` and `auto:BulmaForm` for use with the bootstrap or bulma CSS frameworks.
If you have another favorite CSS framework, it's not hard to make a Form for your needs.
Our `BootstrapForm` and `BulmaForm` are both under 200 lines, and you can start by copying its code.

The following code with create a simple form

```
<auto:Form DataContext="{value: EditedEmployee}" />
```

The control takes the edited view model as `DataContext` and generates form fields for all properties of the object using the metadata from data annotation attributes.

As with grid columns, there is a similar set of properties to customize the form behavior:

* `ExcludeProperties="Id, CreatedTime"`
    - Removes the FirstName and LastName columns from the form
* `IncludeProperties="FirstName, LastName, Email"`
    - Only the listed properties will be included in the form
* `ViewName=Insert` / `GroupName=Group1`
    - Include only properties from the specified group and view
* `Label-LastContactTime="Last Seen"`
    - Sets custom display name for the field
* `Visible-BirthDate={value: IsPerson}`
    - Only display this field if the condition is true
* `Enabled-InvoiceAmount={value: !IsClosed}`
    - Only allow editing this field if the condition is true
* `Changed-Email={staticCommand: _parent.IsEmailUnique = service.IsEmailUnique(Email)}`
    - Event fired when a field changes. May be used to reload some data related to this field
* `<FieldTemplate-X>`
    - overrides the entire field layout (including label, validation, ...)
* `<EditorTemplate-X>`
    - use the template instead of the default editor


If you want to layout the form into multiple parts, you can use the group names to render each group separately. If you specify the `GroupName` property, the `Form` will render
only fields from this group.

```
<!-- This will render two columns. -->
<div class="row">
    <div class="col-md-6">
        <auto:BootstrapForm DataContext="{value: EditedEmployee}" GroupName="Basic Info" />
    </div>
    <div class="col-md-6">
        <auto:BootstrapForm DataContext="{value: EditedEmployee}" GroupName="Contact Info" />
    </div>
</div>
```

If you have implemented your own form control and there is a chance that it might be useful for other people, please send us PR and we'll be happy to include as part of the library.

## Custom Editors

Currently, the framework supports `TextBox`, `CheckBox` and `ComboBox` editors, which can edit string, numeric, date-time and boolean values.
If you want to support any other data type, you can implement your own editor and grid column.

You need to implement the [IFormEditorProvider](./Core/PropertyHandlers/FormEditors/IFormEditorProvider.cs) to implement a custom editor
in the form, and the [IGridColumnProvider](./Core/PropertyHandlers/GridColumns/IGridColumnProvider.cs) to implement about
custom GridView column.

Then, you have to register the editor in the `DotvvmStartup.cs` file. Please note that the order of editor providers and grid columns matters.
AutoUI will use the first provider which returns `CanHandleProperty = true`
for the property.

```
autouiConfig.FormEditorProviders.Add(new YourEditorProvider());
autouiConfig.GridColumnProviders.Add(new YourGridColumnProvider());
```

## Loading Metadata from Resource Files

Decorating every field with the `[Display(Name = "Whatever")]` is not very effective when it comes to localization - you need to specify the resource file type and resource key.
Also, if you have multiple entities with the `FirstName` property, you'll probably want to use the same field name for all of them.

That's why DotVVM Auto UI comes with the resource-based metadata providers. They can be registered in the `DotvvmStartup.cs` like this:

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
* `DisplayFormatAttribute` (`NullDisplayText`)
* `auto:Editor` control for editing individual fields

### In Progress

* More form layouts
* Page templates

### Future

* Auto-generating filters on top of the GridView
