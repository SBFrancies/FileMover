# FileMover

## Description

A console app built in C# with .Net 6 that transfers files from one directory to another directory. Transfers are stored in a local Sqlite DB so that they can be resumed on failure 
of the application.

## Requirements to run

1. .NET 6

## Projects

### FileMover

A console application with the fucntionality to move files from a source directory to a destination directory and track the status of files being transfered. The prject can be configured via an appSettings.json file which should look as follows.

```
{
  "DbLocation":  "default"
}
```

The single configuration options is `DbLocation` which can either be set to a path to an SqlLite .db file or to the value `default` in ehich case a new db file will be created in the user's AppData folder.

### FileMover.UnitTests

This project contains unit tests for the main FileMover project.


### FileMover.IntegrationTests

Contains the integration tests for the project. Integration testing a console app has some quirks and so the majority of the test have been set to `Skip` to avoid potentially unwanted behaviour. Automation testing of on demand test instances in a CI/Cd pipeline would probably be a preferable means to achieve the same outcome if the project remains as a console app.

## Running

1) Download the code from https://github.com/SBFrancies/FileMover

2) In a command window navigate to the project root and run `dotnet publish`

3) Navigate to FileMover\FileMover\bin\Debug\net6.0\publish and double click on `FileMover.exe` to start the application and open a console window. Alternativly use a terminal such as PowerShell and run `.\FileMover.exe` from the containing folder. Alternatively the project can be run directly via Visual Studio 2022.

4) See the parameters section for commands which can be entered.

## Parameters

There are three main commands which can be used in the application. Commands are not case sensitive.

1) `quit` - this exits the application, stopping any file transfers in progress.

2) `status` - this prints a list of file transfers requested during the session (including any resumed from previosu sessions) and their status. Possible statuses are: Awaiting, Copying, Done or Error. If a transfer is in the Error status then an error message explaining the issue will also be displayed.

3) `transfer` - the is the main command which is used to transfer files. It has two arguments: `-s` the source directory and `-d` the destination directory. They must both be specified in that order. For example:

```
transfer -s C:\TestFolder -d D:\NewTestFolder
```
Spaces should not be quoted or escaped, for example:

```
transfer -s C:\Source folder with spaces -d D:\Destination folder with spaces
```

## Notes

Multiple commands can be run during a single session. If a session crashes or is closed those files which have not been trasferred (status is Awaiting or Copying) will be resumed when the application is next started.

The console application itself, takes not arguments, it is only the `transfer` command within the application which has arguments.

## Future development / if I had more time

1) I am not sure that the functionality contained in the application is well suited to a console app. If I were to redevelop it might be as a GUI application or web application. 

2) Further options could be added to the commands. For example at the moment where a file already exists in the destination folder with the same file name as in the source folder the transfer errors. It may be desriable to have a configurable option to overwrite the existing file.

3) There could be more commands for the tracking of files which have already been moved. At the moment while files completed in previous sessions are stored in the DB there is no way to retrieve them via the application.

4) Add other forms of storage. For example an option to use SQL Server rather than Sqlite might make the application more fleixble and improve performance. NoSql storage could also be considered as the data is not currently relational - there is only one table currently.