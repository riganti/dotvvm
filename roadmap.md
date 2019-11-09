## DotVVM Roadmap

Current stable version: **2.3**

Next planned version:   **2.4**

<br />

### DotVVM 2.3

DotVVM 2.1, 2.2 and 2.3 brought many bug fixes and smaller improvements (mostly performance optimizations). **DotVVM 2.4** will continue in this fashion and will also add some experimental features that are planned to be stabilized and finished by DotVVM 3.0.

### DotVVM 3.0

We have some larger features in progress and planning to release them later this year:

* **Client-side GridViewDataSet**: The `GridViewDataSet` API supports sorting and paging of viewmodel collections, but is limited to command bindings and thus is difficult to use with REST API bindings and static commands. We want to introduce a new kind of dataset which would be able to refresh data using REST API binding or a static command, and which would not store the data rows in the viewmodel.

* **State on the server**: One of the obstacles some DotVVM application can hit, is the need to transfer the entire viewmodel from the client to the server. In pages with heavy data-grids, it can slow the application down. Although DotVVM offers some ways how to avoid full postbacks, there are some restrictions. Similarly to server-side Blazor, we are thinking of allowing to store the viewmodel on the server and keep a SignalR connection so the client can only report changes in the viewmodel to the server instead of sending the entire viewmodel.

* **Selenium test helpers**: One of the biggest pain-points in web app development is end-to-end testing. It is difficult to make UI tests resillient to changes made in the UI, and it makes them difficult to maintain. There is a [Selenium Generator](https://github.com/riganti/dotvvm-selenium-generator) project that builds a command-line tool which can browse DotVVM pages and generate C# PageObjects that allow to access page controls in a strongly-typed fashion. When the UI changes and PageObjects are re-generated, you'll immediately see which tests were broken.

* **Electron integration**: Electron can be one of the ways for delivery of multi-platform desktop applications. 

* **WebAssembly integration**: WebAssembly may become the next era of web development, and DotVVM cannot miss the opportunity to benefit from it. We have been thinking about various ways of integrating with WebAssembly. It may be nice to run wasm workloads using a static command binding, for example run a machine learning on an image the user has uploaded.

* **Modularization and refactoring of the JavaScript layer**: We have started refactoring and redesigning the JavaScript part of DotVVM. The new implementation will use ES6 modules and should also streamline and clean up the public JavaScript API. The main benefit is better maintainability and hopefully better performance thanks to splitting the Dotvvm.js file intosmaller pieces and including only these that are really necessary in the page.

* **Better support for PWAs**: DotVVM 2.4 ships with a new feature called "lazy CSRF tokens" which allows to cache generated DotVVM pages or embed them in a mobile app package. The CSRF token is not included in the page and is obtained on the first postback. Also, we plan to implement auto-generation of service worker files that could handle caching for DotVVM routes.

* **Improvements for public web apps**: This includes the improvements in the server rendering mode and other small enhancements. We have also been thinking about automatic generation of AMP versions of particular pages (if the pages don't contain anything that is not supported in AMP).

### Side Projects

There are several side projects with active development. 

* SignalR integration and the ability to update the viewmodel from the server side.
* Translation of C# methods in viewmodels into JavaScript (implemented, but we'll need to redesign it to fit with the rest of DotVVM infrastructure).
* Flex-based layouting control (implemented, will be added as a commercial product).
* [DotVVM Dynamic Data](https://github.com/riganti/dotvvm-dynamic-data)

### Future Ideas

We were thinking of replacing Knockout JS by some vdom based alternative library [#383](https://github.com/riganti/dotvvm/issues/383). Knockout JS would still be supported for compatibility reasons, but the controls included in the framework would not need it. However, the effort of implementing this would be huge, and we are not convinced of the benefits. Also, this wouldn't be possible without many breaking changes.
