# Migrate From HPC Pack SOA to Microsoft Telepathy

## Prerequisite

Make sure your project is target .Net Framework 4.6 or higher.

## Change Assembly Reference

### From HPC Pack 2012 / 2012 R2

- If the project doesn't use any type under `Microsoft.Hpc`  namespace, it is already good to go.

- Otherwise, add reference  to `Microsoft.Telepathy.Session` NuGet package

  > Note: About how to use nightly SDK package, check *Reference `Microsoft.Telepathy.Session` SDK NuGet package* in [README](../readme.md).

### From HPC Pack 2016, 2019 or later

- If the project doesn't reference `Microsoft.HPC.SDK` NuGet package, it is already good to go.

- Otherwise,

  - Remove the reference to `Microsoft.HPC.SDK` NuGet package in NuGet manager

  - Add reference  to `Microsoft.Telepathy.Session` NuGet package

    > Note: About how to use nightly SDK package, check *Reference `Microsoft.Telepathy.Session` SDK NuGet package* in [README](../readme.md).

## Change Source Code

- Remove all using statements for namespace under `Microsoft.Hpc`
- Add missing namespace by
  - Using  IntelliSense of Visual Studio (Recommended), or
  - Add `using Microsoft.Telepathy.Session;`, `using Microsoft.Telepathy.Session.Internal;` (and other referenced namespaces) manually. 
- Try to build the whole solution, and see if there is any build error related to type checking of Session Id. In Telepathy, Session ID is now of type `string` instead of `int` , as `string` can contain much more information. Change all the related type declarations to `string`.

## Change Service Registration File

Service registration file is the file has the same name of your service. e.g. `PrimeFactorizationService.config` for service `PrimeFactorizationService`.

### Change Type Reference

- Change all type reference string under namespace `Microsoft.Hpc.Scheduler.Session.Configuration` to name space `Microsoft.Telepathy.Session.Configuration` 

- Replace all assembly name reference from `Microsoft.Hpc.Scheduler.Session` with `Microsoft.Telepathy.Session`

- Remove culture and version in type reference

  

Example: The type reference

```xml
<section name="service"
         type="Microsoft.Hpc.Scheduler.Session.Configuration.ServiceConfiguration, Microsoft.Hpc.Scheduler.Session, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
         allowDefinition="Everywhere"
         allowExeDefinition="MachineToApplication" />
```

should be changed to

```xml
<section name="service"
         type="Microsoft.Telepathy.Session.Configuration.ServiceConfiguration, Microsoft.Telepathy.Session"
         allowDefinition="Everywhere"
         allowExeDefinition="MachineToApplication" />
```



> You can replace all instance at once using tools like `Replace` in Nodepad.

### Change Service Assembly Path

Change service assembly path to `%TELEPATHY_SERVICE_WORKING_DIR%\<ServiceName>\<ServiceAssemblyName>.dll`.

For example: below assembly path

```xml
<service assembly="\\fileshare\SOAServices\PrimeFactorizationService.dll" />
```

should be changed to

```xml
<service assembly="`%TELEPATHY_SERVICE_WORKING_DIR%\PrimeFactorizationService\PrimeFactorizationService.dll" />
```

