# EarlyBoundGenerator

[![Coverage Status](https://coveralls.io/repos/github/bermo/EarlyXrm.EarlyBoundGenerator/badge.svg?branch=master)](https://coveralls.io/github/bermo/EarlyXrm.EarlyBoundGenerator?branch=master)

This is an early bound class generator for Dynamics 365 which can be easily integrated into 
your Visual Studio projects.

## Features

An example of some C# code generated via the EarlyBoundGenerator are Solution / SolutionComponent entities used internally by the project itself for filtering - [EarlyXrm/EarlyBoundTypes.cs](https://github.com/bermo/EarlyXrm.EarlyBoundGenerator/blob/master/EarlyXrm.EarlyBoundGenerator/EarlyXrm/EarlyBoundTypes.cs)!

The generator extends from the [CrmSvcUtil.exe](https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/org-service/create-early-bound-entity-classes-code-generation-tool)
tool provided by Microsoft in the SDK. It supports the following enhancements:

- installed via nuget so it integrates directly into a visual studio project
- works for SDK style projects
- generates C# classes based on display name which follows the C# Pascal case coding standards
- generates a much cleaner set of C# classes adding using statements and removing duplication
- generates extra attributes on enums to make optionset labels available without requiring an API call
- generates extra attributes on many-to-many relationships to make the intersect entity easily available
- enables inclusion/exclusion of entities and fields via a json configuration settings
- enables inclusion/exclusion of entities and fields via solution/s
- non-global optionset enums can be nested inside their parent entity

## Installation and configuraton

Once installed and configured, an "EarlyXrm" folder will be added to the target project.
By default, this folder contains an EarlyBound.bat file and an earlybound.config file:

    <TargetProject>\EarlyXrm\
                             earlybound.bat
                             earlybound.config

The config file lets you set [configuration](https://github.com/bermo/EarlyXrm.EarlyBoundGenerator/blob/master/EarlyXrm.EarlyBoundGenerator/earlybound.json) options, while the batch file can be used
to easily begin a generation (note, installing a Visual Studio extension such a [Open Command Line](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.OpenCommandLine) can make execution a breeze).


### package.config format

Upon installation, the EarlyXrm should appear straight away. As this package depends
on CoreTools, the bin\coretolls will automatically be included into the project - this folder can be
 excluded/ignored from sourcecontrol as it includes only build-time dependencies

### packagereference format

Package reference style nught packages cannot include non-assemblies on installation.  However other files
can be copied into the project upon build.  

> .NET Framework projects

After package installation, build the target project, and choose "Show All Files", and "Refresh" the
directory.  You should then be able to include the "EarlyXrm" directory. 

> SDK-style projects

After package installation, build the target project, and choose "Show All Files", and "Refresh" the
directory.  You should then be able to include the "EarlyXrm" directory. 

## Connecting to Dynamics

Interactive login is used by default, but a connectionstring property can be set in the configuration file 
which means the generation will kick off immeadiately.  Here are some examples of Dynamics
connection strings that are supported for the cloud:

Client secret (for applications or pipelines):

    "AuthType=ClientSecret;Url=https://?.crm!.dynamics.com/;ClientId=?;ClientSecret=?;"

Username/password (for development only):

    "AuthType=OAuth;Url=https://?.crm!.dynamics.com/;Username=?;Password=?;ClientId={51f81489-12ee-4a9e-aaae-a2591f45987d};RedirectUri=app://58145b91-0c36-4500-8554-080854f2ac97/;"

## Execution

Once the EarlyBoundGenerator has completed, a new generated C# generated file called "EarlyBoundTypes.cs" will appear in the "EarlyXrm" folder (note, for non SDK-style projects, the folder will probably need to be refreshed in Visual Studio). Note, the location and name of the generated file can be modified via the .config file.


