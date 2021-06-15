## DotVVM Roadmap

Current stable version: **3.0**

Next planned version:   **3.1**

<br />

### DotVVM 3.1

There are many features which didn't make it to 3.0, and will be introduced in DotVVM 3.1.

* Enhancements in extension methods discovery
* More LINQ methods supported in data-binding expressions
* Type inference for lambdas in binding expressions
* Validation enhancements
* `FileUpload` control which doesn't need `iframe`

### DotVVM 4.0

We have some ideas on what we can bring into DotVVM 4.0. Here are some of them:

* **Easier way to write controls**: Writing custom controls in DotVVM is difficult, and we'd like to make it really easy. We have implemented a prototype of an easier way to build controls, and we have some ideas that could save many lines of code and make things easier, including a better support for web components and other concepts.

* **(⌛ In progress)** **Selenium test helpers**: One of the biggest pain-points in web app development is end-to-end testing. It is difficult to make UI tests resilient to changes made in the UI, and it makes them difficult to maintain. There is a [Selenium Generator](https://github.com/riganti/dotvvm-selenium-generator) project that builds a command-line tool which can browse DotVVM pages and generate C# PageObjects that allow to access page controls in a strongly-typed fashion. When the UI changes and PageObjects are re-generated, you'll immediately see which tests were broken.

* **Client-side GridViewDataSet**: The `GridViewDataSet` API supports sorting and paging of viewmodel collections, but is limited to command bindings and thus is difficult to use with REST API bindings and static commands. We want to introduce a new kind of dataset which would be able to refresh data using REST API binding or a static command, and which would not store the data rows in the viewmodel.

### Side Projects

There are several side projects with active development. 

* SignalR integration and the ability to update the viewmodel from the server side.
* Translation of C# methods in viewmodels into JavaScript (implemented, but we'll need to redesign it to fit with the rest of DotVVM infrastructure).
* [DotVVM Dynamic Data](https://github.com/riganti/dotvvm-dynamic-data)

### Future Ideas

We were thinking of replacing Knockout JS by some vdom based alternative library [#383](https://github.com/riganti/dotvvm/issues/383). Knockout JS would still be supported for compatibility reasons, but the controls included in the framework would not need it. However, the effort of implementing this would be huge, and we are not convinced of the benefits. Also, this wouldn't be possible without many breaking changes.
