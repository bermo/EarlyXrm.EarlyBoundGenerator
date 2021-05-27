# EarlyBoundGenerator

[![Coverage Status](https://coveralls.io/repos/github/bermo/EarlyXrm.QueryExpressionExtensions/badge.svg?branch=master)](https://coveralls.io/github/bermo/EarlyXrm.EarlyBoundGenerator?branch=master)

This is an early bound class generator for Dynamics 365 which can be easily integrated into 
your Visual Studio projects.

## Features

It uses CrmSvcUtil.exe under the covers, and supports the following features:

- installed via nuget so it integrates directly into a visual studio project
- works for SDK style projects
- generates C# classes based on display name which follows the C# Pascal case coding standards
- generates a much cleaner set of C# classes adding using statements and removing duplication
- generates extra attributes on enums to make optionset labels available without requiring an API call
- generates extra attributes on many-to-many relationships to make the intersect entity easily available
- enables inclusion/exclusion of entities and fields via a json configuration settings
- enables inclusion/exclusion of entities and fields via solution/s
- non-global optionset enums can be nested inside their parent entity

## Connecting

Interactive login is used by default, but a connectionstring property can be set in the configuration file 
which means the generation will kick off immeadiately.  Here are some examples of Dynamics
connection strings that are supported for the cloud:

Client secret (for applications or pipelines):

    "AuthType=ClientSecret;Url=https://?.crm!.dynamics.com/;ClientId=?;ClientSecret=?;"

Username/password (for development only):

    "AuthType=OAuth;Url=https://?.crm!.dynamics.com/;Username=?;Password=?;ClientId={51f81489-12ee-4a9e-aaae-a2591f45987d};RedirectUri=app://58145b91-0c36-4500-8554-080854f2ac97/;"

