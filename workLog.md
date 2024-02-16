# Log of the work done on the project

## Week 5

* Create GitHub organisation and repository.
* Upload original minitwit files to the github repository.
* Create FileOverview.md file were a high-level understanding of the original files is written down.
* Update the Control.sh file with ShellCheck.
* Update dependencies to newest versions.
* Update python version to python 3.
* Change code to support python 3.
* Updated tests to work with python 3.

## Week 6

* Start using our Chirp project from the BDSA course
  - Create Blazor project with .Net template
  - Move backend code over from Chirp project to the new Minitwit blazor project
  - Upgrade .NET version to 8.0.0
  - Upgrade dependencies, and refactor program.cs to adhere to new Asp.Net Core changes 
  - Refactor code to apply new naming project names
  - Reuse workflows from BDSA
  - Change back to razorpages
* Setup project board
  * Added GitHub workflows to automate movement of issues
* Started creating issues for tasks
* Remove OAuth (GitHub Authentication) from the project
* Attempt to setup Dockerfile for the project (Not finished)
  * Attempted to build the Minitwit.Web.csproj, but will now attempt to publish the project using the .sln and use that as an image.