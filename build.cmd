@echo off

pushd %~dp0

.nuget\NuGet.exe update -self

.nuget\NuGet.exe install FAKE -OutputDirectory packages -Version 4.9.1 -ExcludeVersion
.nuget\NuGet.exe install NBench.Runner -OutputDirectory packages -ExcludeVersion -Version 0.2.2
.nuget\NuGet.exe install xunit.runner.console -ConfigFile .nuget\Nuget.Config -OutputDirectory packages\FAKE -ExcludeVersion -Version 2.1.0

if not exist packages\SourceLink.Fake\tools\SourceLink.fsx ( 
  .nuget\nuget.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion
)
rem cls

set encoding=utf-8
packages\FAKE\tools\FAKE.exe build.fsx %*

popd


