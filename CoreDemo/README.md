# CoreDemo
A sample .NET Core project illustrating basic use of:

* ASP.NET MVC
* .NET Core Identity
* Entity Framework
* Custom Logging

The project takes the form of a simple MVC Create/Read/Update/Delete application for a database of Composers
and their respective Works. 

There are two categories of User - Administrator users can add and remove users and view events that are logged
by the application. Database users can view and amend the Composers and Works. 

The applications covers creating new user identities, adding users to roles, declarative authorization of
users to certain sections of the website, code-first use of Entity Framework and a basic custom logger.

## Getting Started


### Prerequisites

The project uses .Net Core 5, so you'll need either the runtime or SDK for [.NET Core 5](https://dotnet.microsoft.com/download/dotnet/5.0).
If you're planning to develop with Visual Studio you'll need an appropriate version.

### Installing

On Windows, you can either use the Git tools built into Visual Studio to clone the repository, or download
and expand a .zip file from Github and then open the .sln file. 

If you're planning to try it out on Linux, use a git client or download and expand a .zip file.

### Usage

Start the project in Visual Studio.
