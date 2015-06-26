ATTENTION: Redwood is currently being renamed to dotVVM
=================================================================================

Redwood is an OWIN-based ASP.NET framework that allows you to write rich client applications without writing javascript code. All you have to do is to write a viewmodel in C# and view in HTML and Redwood will generate the javascript part for you.

Redwood is inspired by ASP.NET WebForms, but it is much more modern. It brings full MVVM experience and it uses KnockoutJS on the client side.


How to start
------------

First download and install the **[Visual Studio Extension](http://riganti.cz/download/Redwood.VS2015Extension_v0.6.zip)**.

Then you can read the tutorial on our **[WIKI](https://github.com/riganti/redwood/wiki)**.



Why to use it?
--------------

+ **Easy to use** - you don't have to know dozens of languages and frameworks like Knockout, Angular, jQuery etc. Just learn HTML, CSS, C# and go.
+ **Stateful controls** - in ASP.NET MVC, writing a custom control that persists its own state an can be easily reused, is quite tricky. With Redwood you can write such components and reuse them multiple times. 
+ **MVVM** - if you have existing WPF or Windows Store app, you can reuse your viewmodels and create a web application and reuse some code. Viewmodels can be also easily tested using unit or integration tests.
+ **Runs on vNext platform** - unlike WebForms, Redwood is not restricted to full .NET and has no COM and IIS dependencies. 
+ **Security** - you can encrypt or sign parts of the viewmodel so the user can not tamper with it. 
+ **Rapid App Development** - with Visual Studio extension, it is quite easy to create a page and a viewmodel. If your application is complicated and contains many form controls, Redwood can handle it quite easily.


Roadmap
-------

+ **Version 0.1**: First working demo. Basic controls, postback support
+ **Version 0.2**: Few basic samples, master pages, simple stateless markup controls
+ **Version 0.3**: Stateful controls, Validation
+ **Version 0.4**: Postback.Update and RenderSettings.Mode properties (server side rendering)
+ **Version 0.5**: Visual Studio extension
+ **Version 0.6 (current)**: Stable version - more controls, several bug fixes
+ **Version 0.7**: DNX support, bug fixes
+ **Version 0.8**: Styles, more framework features and other controls
+ **Version 0.9**: Automated translation of C# ViewModel commands to javascript
