## How to contribute

Like in any open source project, there is a ton of things to do in DotVVM, from coding smaller features, addressing flaws to engineering new complex areas of functionality. Whether you like designing compilers, tweaking runtime performance, metaprogramming user interface or messing with unmanaged memory, there is something for you. :)

First, start by choosing what you'd like to do - you can have a look at [issues up-for-grabs label](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs+sort%3Areactions-%2B1-desc). We have also prepared other possibilities for you to have a look at listed below. Then fork the repo, clone it locally and your are ready to develop. Finally, create a pull request to the master branch, make sure that you have merged the latest changes and that the PR has a readable diff (please avoid reformatting documents, refactoring stuff, changing CRLF to LF or tabs to spaces etc.).

### Which are the things I can work on?

* [up-for-grabs issues](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs+sort%3Areactions-%2B1-desc) - There is sometimes a brief description how it could be implemented, but feel free to ask if anything is not clear, we will be happy to help.
* If you'd like to implement your own feature, we recommend to describe it first in an issue, so we can discuss it and help you with implementation. Sometimes the feature might be already implemented, but it just has a different name.
* We have a [dotvvm-contrib](https://github.com/riganti/dotvvm-contrib) repository which contains several community-created controls. We'd love to see more of them. There is a brief contributing guide in the repo.
* Tooling - we already have quite powerful [Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=TomasHerceg.DotVVMforVisualStudio-17892), but there is no support for other popular editors like **Visual Studio Code**. Actually, there is a one-evening experiment https://github.com/riganti/dotvvm-extension-vscode, so you can start from it. If you'd have any questions, I'd recommend you to mention @Mylan719, the author of our Visual Studio Extension.
* If you'd just like to try using DotVVM in an application instead of hacking its internals, feel free to publish your work on github, we will be happy to link it as a sample application. We would also appreciate any feedback from you.
* If you like fixing things in a documentation, it is located at [riganti/dotvvm-docs](https://github.com/riganti/dotvvm-docs)
* You can also take a look at performance optimization and testing - we have a [repository with benchmarks](https://github.com/riganti/dotvvm-benchmarks) which measure time per request of all our samples. Also have a look at https://github.com/riganti/dotvvm/issues/170 and at [a performance report](https://ipfs.io/ipfs/QmScnYdY8xoPeHPN85edPdLPbi3GvHrUGicvHAuyMdrAQE/reports/BenchmarkRun-001-2017-05-31-10-34-59/report.html)
* [DotVVM Dynamic Data](https://github.com/riganti/dotvvm-dynamic-data) - a library that generates user interface from reflection metadata and data annotation attributes. This is how you do a form for the EditedEmployee property `<dd:DynamicEntity DataContext="{value: EditedEmployee}" />`. You can have a look at the Roadmap in the README.md, make it faster or try to use it and fix all the problems that you have with it ;)
* No framework can exist without a large amount of tutorials, blog posts and screencasts. Creating this type of content and publishing it anywhere people can find it, is really appreciated. 
* You can always just participate in a discussion with your ideas, see [issue marked as proposal](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3Aproposal+sort%3Acomments-desc). It is often actually much harder to decide how the API should look like than to do the "real coding", so you may help us a lot.

### Project Setup

To get started, fork the repository, and clone it on or computer. You can use Visual Studio's Team Explorer window or run `git clone https://github.com/<your_github_username>/dotvvm.git`. 

We are using Visual Studio 2017 and some projects use .NET Core 2.0, so the solution will probably not open in previous versions of Visual Studio. 

Open the `src/DotVVM.sln` solution in Visual Studio, or open the `src` folder in VS Code. Set `Samples/DotVVM.Samples.BasicSamples.AspNetCore` as a startup project and launch it. It should just work. If it does not, feel free to ask. For VS Code, you can try this [launch.json](https://gist.github.com/exyi/860821793b617b3ed0c9c9bb91157111) and [tasks.json](https://gist.github.com/exyi/3055cd3cec7d246475d660d1ec82a7fd). You can also try to rebuild `Tests/DotVVM.Framework.Tests.Common` project and run the unit/integration tests - it should complete in few seconds, everything should be green. :)

Almost everying is in the `DotVVM.Framework` project, except few base interfaces and attributes in `DotVVM.Core` (so you don't have to reference the entire framework in your bussiess layer, if you just want to suppress serialization or turn on the validation of certain properties). 

The OWIN and ASP.NET Core integration is splitted in two projects called `DotVVM.Framework.Hosting.AspNetCore` and `...Owin`, so you don't have to reference Owin in your AspNetCore project and vice versa. 

`DotVVM.Compiler` project is a command line application that can be used for view precompilation and is used by our VS Extension to dump configuration from the project after the DotvvmStartup has run. 

`DotVVM.CommandLine` is a new project that add `dotvvm` subcommand to the `dotnet` CLI, [more info in a blog post](https://www.dotvvm.com/blog/17/DotVVM-1-1-RC-5-dotnet-new-and-DotVVM-CLI). 

## Communication

If you have any questions or want to ask anything, you can use a [Gitter chat](https://gitter.im/riganti/dotvvm) or post a comment in the issue you'd like to work on. We have the Czech chat at [Gitter](https://gitter.im/riganti/dotvvm-cz) as well.