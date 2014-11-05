Redwood
=======

Redwood is an OWIN-based ASP.NET framework that allows you to write rich client applications without writing javascript code. All you have to do is to write a viewmodel in C# and Redwood will generate the javascript part for you.

Redwood is inspired by ASP.NET WebForms, but it is much more modern. It brings full MVVM experience and it uses KnockoutJS on the client side.


Why to use it?
--------------

+ **Easy to use** - you don't have to know dozens of languages and frameworks like Knockout, Breeze, jQuery etc. Just learn HTML, CSS, C# and go.
+ **Stateful controls** - writing a smart control that has its own state in ASP.NET MVC is quite tricky. Redwood allows to write rich components that seamlessly persist their state in the viewmodel.
+ **Runs on vNext platform** - unlike WebForms, Redwood is not restricted to full .NET and has no COM and IIS dependencies. 


Why it is better than WebForms?
-------------------------------

+ **No ViewState** - your viewmodel is the new ViewState. You have direct control on what is transferred between server and client. You can also use stateless commands and if some command doesn't require to be executed on the server, it is translated to javascript code and executed locally.
+ **Testable** - testing Redwood app is no pain. You just create your viewmodel instance by yourself, set the properties, call the commands and verify the results.
+ **Clean HTML** - Redwood controls generate clean and simple HTML that can be fully styled using CSS. No more hours spent by persuading asp:Calendar to not render inline styles with hard coded colors.


Roadmap
-------

+ **Version 0.1 (current)**: First working demo. Basic controls, postback support.
+ **Version 0.11**: Few samples, basic Visual Studio integration.
+ **Version 0.2**: Stateful controls, master pages and server-side rendering (analogy of WebForms UpdatePanel).
+ **Version 0.3**: Validation, Xamarin support for hosting Redwood in mobile apps
+ **Version 0.4**: More controls (Bootstrap support), translation of C# viewmodel commands to javascript



