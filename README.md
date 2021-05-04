# EarlyBoundGenerator

This is an early bound class generator for Dynamics 365 which can be easily integrated into 
your Visual Studio projects.

## Features

It uses CRMSvcUtil.exe under the covers, and supports the following features:

- installed via nuget so it integrates directly into a visual studio project
- works for SDK style projects
- generates C# classes based on display name which follows the C# Pascal case coding standards
- generates a much cleaner set of C# classes adding using statements and removing duplication
- generates extra attributes on enums to make optionset labels available without requiring an API call
- generates extra attributes on many-to-many relationships to make the intersect entity easily available
- enables inclusion/exclusion of entities and fields via a json configuration settings
- enables inclusion/exclusion of entities and fields via solution/s
- non-global optionset enums can be nested inside their parent entity
