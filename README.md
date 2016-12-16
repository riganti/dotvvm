DotVVM = HTML + C#
==================

## .NET-based Framework for Modern Web Apps


[![license](https://img.shields.io/github/license/riganti/dotvvm.svg?maxAge=2592000?style=plastic)]()
[![Join the chat at https://gitter.im/riganti/dotvvm](https://badges.gitter.im/riganti/dotvvm.svg)](https://gitter.im/riganti/dotvvm)

![TFS Build: ](https://rigantitfs.visualstudio.com/_apis/public/build/definitions/8dfab054-d6f0-471d-88c2-4f230395cdd1/4/badge)

**DotVVM** is an ASP.NET framework that lets you build **line-of-business applications** and **SPAs** without writing tons of JavaScript code. You only have to write a viewmodel in C# and a view in HTML. DotVVM will do the rest for you.

**DotVVM** brings full **MVVM** experience and it uses **KnockoutJS** on the client side. It handles the client-server communication, validation, localization, date & time formatting on the client side, SPAs and much more. 

It is open source, it supports both OWIN and ASP.NET Core and it runs on .NET Framework, .NET Core and Mono.
It also offers a free extension for Visual Studio 2015 with IntelliSense and other useful features which make the development really easy and productive.  

<br />

How to Start
------------

1. Install the **[DotVVM for Visual Studio 2015](https://www.dotvvm.com/landing/dotvvm-for-visual-studio-extension)** extension.

2. Read the **[documentation](http://www.dotvvm.com/docs)**. 

<br />

Simple Sample
-------------
DotHTML markup: 
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

ViewModel in C#:
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


More Info
---------

You'll find more information on our website [DotVVM.com](https://www.dotvvm.com).
