@echo off

pushd %~dp0

.nuget\NuGet.exe update -self

.nuget\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion -Version 3.28.8

if not exist packages\SourceLink.Fake\tools\SourceLink.fsx ( 
  .nuget\nuget.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion
)
rem cls

set encoding=utf-8
packages\FAKE\tools\FAKE.exe build.fsx %*

popd


