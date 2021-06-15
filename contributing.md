## How to contribute

There are many ways how you can help the DotVVM project - it doesn't need to be only a pull request to the main repo. You can spread the word about DotVVM in a blog post, forum or a user group. You can build a demo app in DotVVM and publish it on GitHub. You can send us feedback, fix a typo or add more pages in our [Docs repository](https://github.com/riganti/dotvvm-docs), submit an issue or suggest a new feature you'd like.

If you want to dig in the code, like in any open source project, there is a ton of things to do in DotVVM: from implementing smaller features, addressing flaws to engineering new complex areas of functionality. Whether you like designing compilers, tweaking runtime performance, metaprogramming user interface or messing with unmanaged memory, there is something for you. :)

You will need to sign a [Contributor License Agreement](https://cla.dotnetfoundation.org/) before submitting your pull request. We kindly ask you to respect the [Code of Conduct](code-of-conduct.md). 

### Interesting repositories

* [Main DotVVM repo](https://github.com/riganti/dotvvm) - the framework itself + tests
* [DotVVM Docs repo](https://github.com/riganti/dotvvm-docs) - markdown files from which we generate our [docs](https://dotvvm.com/docs)
* [DotVVM Contrib repo](https://github.com/riganti/dotvvm-contrib) - community-authored DotVVM components

--- 

### Digging in the code

First, start by choosing what you'd like to do - you can have a look at [issues with "good first issue" label](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22). We have also prepared other possibilities for you to have a look at listed below. 

Then, you can fork the repo, clone it locally, and you are ready to develop. 

Finally, you'll need to create a pull request to the `v3-master` (for DotVVM 3.0) or `master` branch (for DotVVM 2.5) (__we can help with that__), make sure that you have merged the latest changes and that the PR has a readable diff.

 **Please avoid reformatting documents, refactoring stuff, changing CRLF to LF or tabs to spaces etc. - it makes the PRs difficult to review.** There is definitely room for refactoring or cleanup, but it should be done in a separate PR so it is not messed up with functional changes.

### Which are the things I can work on?

* [good first issues](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22) - There is sometimes a brief description how it could be implemented, but feel free to ask us on [Gitter](https://gitter.im/riganti/dotvvm) if anything is not clear, we will be happy to help. We can set up a call and explain the issue, help with implementation, testing or creating the PR. 
* If you'd like to implement your own feature, we recommend to describe it first in an issue, so we can discuss it and help you with implementation. Sometimes the feature might be already implemented, but it just has a different name.
* We have a [DotVVM Contrib](https://github.com/riganti/dotvvm-contrib) repository which contains several community-authored components. We'd love to see more of them. There is a brief contributing guide in the repo.
* Feedback for the [ASP.NET Web Forms modernization story using DotVVM](https://dotvvm.com/modernize) - DotVVM can be used to modernize old ASP.NET web apps without rewriting them completely. We'll be happy if you try DotVVM on your Web Forms app and give us feedback, or possibly write an article or a case study. As a reward, we can help you with the migration if you run into any difficulties - just contact us on [Gitter](https://gitter.im/riganti/dotvvm). 
* Tooling - we already have quite powerful [Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=TomasHerceg.DotVVMforVisualStudio-17892), but there is only a limited support for **Visual Studio Code** and no for other popular editors. We have a simple [VS Code Extension](https://github.com/riganti/dotvvm-extension-vscode), so you can start from it. If you have any questions, I'd recommend you to mention @Mylan719, the author of our Visual Studio Extension. 
* You can also take a look at performance optimization and testing - we have a [repository with benchmarks](https://github.com/riganti/dotvvm-benchmarks) which measure time per request of all our samples. Also have a look at https://github.com/riganti/dotvvm/issues/170 and at [a performance report](https://ipfs.io/ipfs/QmScnYdY8xoPeHPN85edPdLPbi3GvHrUGicvHAuyMdrAQE/reports/BenchmarkRun-001-2017-05-31-10-34-59/report.html)
* [DotVVM Dynamic Data](https://github.com/riganti/dotvvm-dynamic-data) - a library that generates user interface from reflection metadata and data annotation attributes. This is how you do a form for the `EditedEmployee` property `<dd:DynamicEntity DataContext="{value: EditedEmployee}" />`. You can have a look at the roadmap in the README.md, make it faster or try to use it and fix all the problems that you have with it. ;)
* If you'd just like to try using DotVVM in an application instead of hacking its internals, feel free to publish your work on GitHub, we will be happy to link it as a sample application. We would also appreciate any feedback to the framework.
* If you like fixing things in a documentation, it is located at [riganti/dotvvm-docs](https://github.com/riganti/dotvvm-docs)
* No framework can exist without a large amount of tutorials, blog posts and videos. Creating this type of content and publishing it anywhere people can find it, is really appreciated. We'll be happy to share your content on our social media and in newsletters. You can also become a member of our [DotVVM Developer Advocate](https://www.dotvvm.com/blog/67/Introducing-DotVVM-Developer-Advocates) group - they write articles, organize virtual meetups and events and help with spreading the word about DotVVM. 
* You can always just participate in a discussion with your ideas, see [issue marked as proposal](https://github.com/riganti/dotvvm/issues?q=is%3Aopen+is%3Aissue+label%3Aproposal+sort%3Acomments-desc). It is often actually much harder to decide how the API should look like than to do the "real coding", so you may help us a lot.

### Project setup

If you plan to work with the DotVVM repository, here is a short manual what you need to do.

To get started, fork the repository, and clone it on or computer. You can use Visual Studio's Team Explorer window or run `git clone https://github.com/<your_github_username>/dotvvm.git`. 

We are using Visual Studio 2019 or Visual Studio Code. Some projects use .NET Core 2.0 and .NET Core 3.1, so the solution will probably not open in previous versions of Visual Studio. 

Open the `src/DotVVM.sln` solution in Visual Studio, or open the `src` folder in VS Code. 

Set `Samples/DotVVM.Samples.BasicSamples.AspNetCore` as a startup project and launch it. It should just work - you'll see a page with a long list of samples (they are not often meaningful, they are used by the UI tests to verify all framework features work; however they are great for playing). 

If the project does not start, feel free to ask us on [Gitter](https://gitter.im/riganti/dotvvm). For VS Code, launch it from the `src` directory, so it can find the `.vscode/launch.json` and `.vscode/tasks.json` files. You can also try to rebuild `Tests/DotVVM.Framework.Tests.Common` project and run the unit/integration tests - it should complete in few seconds, everything should be green. :)

Almost everything interesting is in the `DotVVM.Framework` project, except for some base interfaces and attributes in `DotVVM.Core` (so you don't have to reference the entire framework in your business layer, if you just want to suppress serialization or turn on the validation of certain properties). 

The OWIN and ASP.NET Core integration is split in two projects called `DotVVM.Framework.Hosting.AspNetCore` and `...Owin`, so you don't have to reference Owin in your ASP.NET Core project and vice versa. 

`DotVVM.Compiler` project is a command line application that can be used for view precompilation and is used by our VS Extension to dump configuration from the project after the DotvvmStartup has run. 

`DotVVM.CommandLine` is a new project that add `dotvvm` subcommand to the `dotnet` CLI, [more info in a blog post](https://www.dotvvm.com/blog/17/DotVVM-1-1-RC-5-dotnet-new-and-DotVVM-CLI). 

### Linking DotVVM from your app

You may want to try to use DotVVM source codes directly from your project so you can interactively test your changes or simply check if some bugfix works correctly. 

The first step is to clone the DotVVM git repository, if you want to make some changes, you probably want to fork in on github beforehand (see above for more info). The second and last step is to replace NuGet reference to project reference in your project file (`Something.csproj`) - simply replace the `<PackageReference Include="DotVVM.AspNetCore" ... />` with `<ProjectReference Include="path/to/dotvvm/src/DotVVM.Framework.Hosting.AspNetCore/DotVVM.Framework.Hosting.AspNetCore.csproj" />`. In VS Code, you don't need to update solutions file or anything, this is the only thing the .NET SDK cares about - in Visual Studio 2019, you'll probably need to add DotVVM projects to the solution (right-click and Add Existing Project).

## Support and communication

If you have any questions or want to ask anything, you can use a [Gitter chat](https://gitter.im/riganti/dotvvm) or post a comment in the issue you'd like to work on. If the topic is more complicated, we'd be happy to set up a call and discuss it.

We have the Czech chat at [Gitter](https://gitter.im/riganti/dotvvm-cz) as well.

We'd be grateful for any contribution. Please, be polite and follow our [Code of conduct](https://github.com/riganti/dotvvm/blob/master/code-of-conduct.md).

