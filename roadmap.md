## DotVVM Roadmap

Current stable version: **4.3**

Next planned version:   **5.0**

<br />

### DotVVM 5.0

DotVVM 5.0 adds focuses on providing extensibility points and contains significant performance improvements:

* Generic `GridViewDataSet` that allows multi-criteria sorting, token-based paging, and more
* `AppendableDataPager` control that can create infinite scrolling experience
* `ModalDialog` control based on native HTML `<dialog>` element
* Replacing `Newtonsoft.Json` with `System.Text.Json` that should bring significant performance improvements
* Using `resource` bindings in `Repeater` and `GridView` controls

### Plans for next major versions

* Reimplementation of the Visual Studio extension to run out-of-process and adding support to Visual Studio Code and JetBrains Rider using Language Server Protocol
* Enhancing [ASPX to DotVVM converter](https://dotvvm.com/webforms/convert) to support more translations and use of AI
* Simplifying the route and control registration to avoid over-complicated `DotvvmStartup.cs` file
* Adding support for ASP.NET Core [static assets](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0) (pre-compressed images, scripts, stylesheets etc.)
* Support for polymorphism in viewmodels and the `as` keyword

