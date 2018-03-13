DotVVM = HTML + C#
==================

## .NET-based MVVM Framework for Web Apps


[![license](https://img.shields.io/github/license/riganti/dotvvm.svg?maxAge=2592000?style=plastic)]()
[![Join the chat at https://gitter.im/riganti/dotvvm](https://badges.gitter.im/riganti/dotvvm.svg)](https://gitter.im/riganti/dotvvm)

![TFS Build: ](https://rigantitfs.visualstudio.com/_apis/public/build/definitions/8dfab054-d6f0-471d-88c2-4f230395cdd1/4/badge)

[DotVVM](https://www.dotvvm.com) is an ASP.NET framework that lets you build **line-of-business applications** and **SPAs** without writing tons of JavaScript code. You only have to write a viewmodel in C# and a view in HTML. DotVVM will do the rest for you.

**DotVVM** brings full **MVVM** experience and it uses **KnockoutJS** on the client side. It handles the client-server communication, validation, date & time formatting on the client side, SPAs and much more. 

It is open source, it supports both OWIN and ASP.NET Core and it runs on **.NET Framework**, **.NET Core** and **Mono**.

It also offers a [free extension for Visual Studio 2015 and 2017](https://www.dotvvm.com/install) with IntelliSense and other useful features which make the development really easy and productive.  

<br />

How To Use
----------

The **Views** in DotVVM use HTML syntax with __controls__ and __data-bindings__.

```html
<div class="form-control">
  <dot:TextBox Text="{value: Name}" />
</div>
<div class="form-control">
  <dot:TextBox Text="{value: Email}" />
</div>
<div class="button-bar">
  <dot:Button Text="Submit" Click="{command: Submit()}" />
</div>
```

The **ViewModels** are plain C# objects. You can call public methods from the **View**.

```C#
public class ContactFormViewModel
{
  public string Name { get; set; }
  public string Email { get; set; }
  public void Submit()
  {
    ContactService.Submit(Name, Email);
  }
}
```

You just need to know C#, HTML and CSS. For most scenarios you don't have to write any JavaScript code. If you are wondering what is going on, see the "How Does it Work" section below.

<br />


Features of DotVVM (Open Source & Free for Everyone)
----------------------------------------------------

* Many built-in controls
    + [GridView](https://www.dotvvm.com/docs/controls/builtin/GridView/latest), [Repeater](https://www.dotvvm.com/docs/controls/builtin/Repeater/latest)
    + [FileUpload](https://www.dotvvm.com/docs/controls/builtin/FileUpload/latest)
    + [TextBox](https://www.dotvvm.com/docs/controls/builtin/TextBox/latest), [ComboBox](https://www.dotvvm.com/docs/controls/builtin/ComboBox/latest), [CheckBox](https://www.dotvvm.com/docs/controls/builtin/CheckBox/latest), [RadioButton](https://www.dotvvm.com/docs/controls/builtin/RadioButton/latest)
    + [Button](https://www.dotvvm.com/docs/controls/builtin/Button/latest), [LinkButton](https://www.dotvvm.com/docs/controls/builtin/LinkButton/latest), [RouteLink](https://www.dotvvm.com/docs/controls/builtin/RouteLink/latest)
    + [Validator](https://www.dotvvm.com/docs/controls/builtin/Validator/latest), [ValidationSummary](https://www.dotvvm.com/docs/controls/builtin/ValidationSummary/latest)
    + ...
* [Advanced validation rules](https://www.dotvvm.com/docs/tutorials/basics-validation/latest) integrated with .NET data annotation attributes
* Support for [.NET cultures](https://www.dotvvm.com/docs/tutorials/basics-globalization/latest), number & date formats and RESX localization
* [SPA (Single Page App)](https://www.dotvvm.com/docs/tutorials/basics-single-page-applications-spa/latest) support
* [User controls](https://www.dotvvm.com/docs/tutorials/control-development-introduction/latest)
* MVVM with [testable ViewModels](https://www.dotvvm.com/docs/tutorials/advanced-testing-viewmodels/latest) and [IoC/DI support](https://www.dotvvm.com/docs/tutorials/advanced-ioc-di-container/latest)
* [Visual Studio integration with IntelliSense](https://www.dotvvm.com/landing/dotvvm-for-visual-studio-extension)
* [OWIN](https://www.dotvvm.com/docs/tutorials/how-to-start-dotnet-451/latest) and [ASP.NET Core](https://www.dotvvm.com/docs/tutorials/how-to-start-dnx/1-1) support
* [DotVVM Dynamic Data](https://github.com/riganti/dotvvm-dynamic-data)

<br />

Need More? We have Commercial Controls!
---------------------------------------

* [Bootstrap for DotVVM](https://www.dotvvm.com/landing/bootstrap-for-dotvvm) - more than 40 controls that make using Bootstrap easier and your code much cleaner
* [DotVVM Business Pack](https://www.dotvvm.com/landing/business-pack) - Enterprise ready controls for Line of business web apps

<br />

How to Start
------------

1. Install the **[DotVVM for Visual Studio](https://www.dotvvm.com/landing/dotvvm-for-visual-studio-extension)** extension.

2. Read the **[documentation](http://www.dotvvm.com/docs)**. 

How Does it Work
----------------
DotVVM is no magic, so let's have a look at how it works. Or at least how would a simplified core work.

### Page Load

When the page is requested, DotVVM will process the dothtml markup into a control tree - a tree made of `DotvvmControl` instances which correspond to the structure of dothtml. In the meantime, your ViewModel is initialized so data bindings can be evaluated on the server. Then, the page is "rendered" to HTML with knockout `data-bind`ings - each DotvvmControl handles the rendering of its properties and can decide if the data bindings should be evaluated on the server or translated to a Javascript expression (or both). The ViewModel is serialized to JSON and included in the page.

On the client side, after the page is loaded the ViewModel is deserialized and used as a knockout model. When you touch the page (edit a textbox or so) the changes are assigned back to the knockout model - it always represents the page's current state.

### Command bindings

We have two types of commands in DotVVM - the powerful and expansive `command` and its lighter counterpart `staticCommand`.

When a [`command`](https://www.dotvvm.com/docs/tutorials/basics-command-binding/latest) is invoked a "postback" is dispatched - the entire ViewModel is serialized and sent to the server. Here, the page is created again, ViewModel is deserialized, the expression in the binding is invoked, the ViewModel is serialized and sent back to the client. Note that you can control which parts of the ViewModel are sent using the [`Bind` attribute](https://www.dotvvm.com/docs/tutorials/basics-binding-direction/latest).

A [`staticCommand`](https://www.dotvvm.com/docs/tutorials/basics-static-command-binding/latest) is slightly different as the binding expression is not invoked on the server but instead is translated to Javascript and invoked client-side. Only when you use a function that is not translatable to JS and is marked with an `AllowStaticCommand` attribute the request to the server is dispatched. However, it is not the full postback - it will only contain the function's arguments. On the server, the function is going to be invoked (with the deserialized args) and only its result will be sent back to the client. When the response returns, the rest of the expression will be evaluated. If you'd have a look at the JS generated from your staticCommand, you would find an ugly expression that invokes some function on the server and processes the results in the callback.

<br />

More Info
---------

* [DotVVM.com](https://www.dotvvm.com)
* [DotVVM Blog](https://www.dotvvm.com/blog)
* [Documentation](https://www.dotvvm.com/docs)
* [Twitter @dotvvm](https://twitter.com/dotvvm)
* [Gitter Chat](https://gitter.im/riganti/dotvvm)
* [Samples](https://github.com/search?q=topic%3Adotvvm-sample+org%3Ariganti&type=Repositories)
* [Roadmap](https://github.com/riganti/dotvvm/blob/master/roadmap.md)
