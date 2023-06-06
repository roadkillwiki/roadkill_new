# .NET Core Roadkill (v3)

This was a new repository to make the size of the Git repository smaller, and eventually move the main Roadkill repository here. It was a greenfield upgrade of Roadkill to .NET Core.

**While this project is 99% functionally complete, it stopped at .NET 5. It hasn't been continued because of the large amount of work involved with integrating an OAuth 2 solution, and rewriting the front-end as a SPA with React or similar.**

Feel free to fork and roll your own front end!

### Original Roadmap

#### ASP.NET Core

The next version will be ASP.NET Core only, aimed at Linux hosting for cost and scability.

#### Enhanced security

Through its ASP.NET Core identity integration, version 3 will support everything that `Microsoft.AspNetCore.Identity` [supports](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x)

#### Postgres only

Version 3 will be Postgres only, using [Marten](http://jasperfx.github.io/marten/) as its NoSQL document store. Postgres can be run as Docker container, or is available a service by services such as AWS RDS.

#### API-first

The new Roadkill will be powered by its RESTful API, rather than version's 2 after-thought approach. This enables far easier plugin and extensibility to exist for Roadkill.

#### Docker

Roadkill 3 will be a docker image you run, on Linux Docker. With Docker comes built in scalibility, easier versioning, 

#### Postgres for searching

The next version will use Postgres for its search engine, rather than Lucene, removing a lot of complexity and past problems from Roadkill.

#### No more Creole support
Sorry Creole fans, but supporting 3 different markdown formats is too labour intensive, and CommonMark has come a long and pretty much made Creole redundant, and Mediawiki syntax has zero support for .NET. Looking at commercial wiki engines like Confluence, it ultimately doesn't make a lot of difference what markdown format you support, providing there is good documentation for the syntax.

Version 3 will only support Markdown, using the [CommonMark](http://commonmark.org/) standard via [Markdig](https://github.com/lunet-io/markdig). CommonMark is a well thought-out and documentated extension of Markdown and has a large community behind it.

#### Improved editing experience
Because Roadkill is moving to CommonMark, the editor can now be improved to be more user friendly, and have a faster client-side preview. The TUI editor is currently being considered for this: https://github.com/roadkillwiki/roadkill/issues/57

#### A new theme
A new material-design based theme.

#### Re-designed file manager
While this may not make the initial v3 release, the plan is to redesign the file manager to emulate the Wordpress 4 file manager.

#### Better page dialog for adding links
Instead of having to memorize page names, adding links will be similar to Wordpress in finding pages that exist on the site.
