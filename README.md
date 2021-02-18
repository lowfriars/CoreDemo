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

I found a bunch of problems installing only the runtime on Linux which were solved by installing the SDK,
so to be on the safe side, go for the SDK if you have the space.

If you're planning to develop with Visual Studio you'll need an appropriate version.

### Installing

On Windows, you can either use the Git tools built into Visual Studio to clone the repository, or download
and expand a .zip file from Github and then open the .sln file. 

If you're planning to try it out on Linux, use a git client or download and expand a .zip file.

### Usage

You can either navigate to the directory containing the project file (CoreDemo.csproj) and 
type ``dotnet run`` (you might have to issue the command ``dotnet restore`` beforehand) or open
the solution file in Visual Studio and run the project from there.

If you use the command line, you'll see some lines that say ``now listening on`` - these are the local web
addresses where you will the application running (one for http and one for https), usually <http://localhost:5000>
and <https://localhost:5001>. You'll automatically be redirected to the ``https`` link is you pick the other one.

The default webserver uses a self-signed cerificate, so you'll get a security warning which you will 
have to bypass however hard the browser tries to warn you otherwise.

There is a "Log in" link in the right hand part of the menu bar and you can log in 
with the username "Admin1" and password "@#Zz1234". You will be prompted to change your 
password. When you've logged in there you will see links for "Users", "Event Log" and "Composers".

Under "Users" you can create additional users and give them roles of "Adminstrator" or "Database".
Administrators are not authorised to access the database of Composers and Works and people in the Database
role can't amend the users, so if you click on "Composers" you should be redirected to a page that
indicates access is forbidden. You can check the Event Log to see that this has happened.

If you create a "Database" user and log in to that account, the "Users" and "Event Log" are missing from
the menu - but if you try to type in a URL such as <https://localhost:5001/Log> you'll also get the "Forbidden"
message. You can click on "Composers" and add a composer and some works they may have written.

The SQLite database is stored in a file named ``demodata.db`` - you can reset the system
to its original state by copying ``demodata-blank.db`` over it.
