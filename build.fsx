#I @"packages/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open Fake
open Fake.FileUtils
open Fake.TaskRunnerHelper

//--------------------------------------------------------------------------------
// Information about the project for Nuget and Assembly info files
//-------------------------------------------------------------------------------

let product = "Helios"
let authors = [ "Aaron Stannard" ]
let copyright = "Copyright © Aaron Stannard 2013-2015"
let company = "Helios Framework"
let description = "High performance, reactive TCP / UDP socket middleware for .NET"
let tags = ["socket";"sockets";"helios";"UDP";"TCP";"Netty";"reactor"]
let configuration = "Release"

// Read release notes and version
let parsedRelease =
    File.ReadLines "RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes

let envBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER")
let buildNumber = if String.IsNullOrWhiteSpace(envBuildNumber) then "0" else envBuildNumber

let version = parsedRelease.AssemblyVersion + "." + buildNumber
let preReleaseVersion = version + "-beta"

let isUnstableDocs = hasBuildParam "unstable"
let isPreRelease = hasBuildParam "nugetprerelease"
let release = if isPreRelease then ReleaseNotesHelper.ReleaseNotes.New(version, version + "-beta", parsedRelease.Notes) else parsedRelease


//--------------------------------------------------------------------------------
// Directories

let binDir = "bin"
let testOutput = "TestResults"

let nugetDir = binDir @@ "nuget"
let workingDir = binDir @@ "build"
let nugetExe = FullName @".nuget\NuGet.exe"

open Fake.RestorePackageHelper
Target "RestorePackages" (fun _ -> 
     "./Helios.sln"
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = "./packages"
             Retries = 4 })
 )

//--------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    DeleteDir binDir
)

//--------------------------------------------------------------------------------
// Generate AssemblyInfo files with the version for release notes 


open AssemblyInfoFile

Target "AssemblyInfo" (fun _ ->
    let version = release.AssemblyVersion

    CreateCSharpAssemblyInfoWithConfig "src/SharedAssemblyInfo.cs" [
        Attribute.Company company
        Attribute.Copyright copyright
        Attribute.Version version
        Attribute.FileVersion version ] <| AssemblyInfoFileConfig(false)
)

//--------------------------------------------------------------------------------
// Build the solution

Target "Build" (fun _ ->
    !!"Helios.sln"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

//--------------------------------------------------------------------------------
// Copy the build output to bin directory
//--------------------------------------------------------------------------------

Target "CopyOutput" (fun _ ->    
    let copyOutput project =
        let src = "src" @@ project @@ "bin" @@ "Release" 
        let dst = binDir @@ project
        CopyDir dst src allFiles
    [ "Helios"
      "Helios.NET35"
      "Helios.NET40"
    ]
    |> List.iter copyOutput
)

Target "BuildRelease" DoNothing
Target "BuildReleaseMono" DoNothing

//--------------------------------------------------------------------------------
// Tests targets
//--------------------------------------------------------------------------------
Target "RunTests" <| fun _ ->
    let nunitAssemblies = !! "tests/**/bin/Release/*.Tests.dll" ++ "tests/**/bin/Release/Helios.MultiNodeTests.dll"

    mkdir testOutput
    nunitAssemblies |> NUnit(fun p -> 
        {p with 
            DisableShadowCopy = true;
            OutputFile = testOutput + "TestResults.xml" })

//--------------------------------------------------------------------------------
// Clean test output

Target "CleanTests" <| fun _ ->
    DeleteDir testOutput

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------
module Nuget = 

    let getProjectBinaries project =
        match project with
        | "Helios" -> [
                        "Helios"
                        "Helios.NET40"]
        | "Helios.NET35" -> ["Helios.NET35"]
        | e -> [e]

    let getProjectBinFolders project =
        match project with
        | "Helios" -> "lib" @@ "net45"
        | "Helios.NET40" -> "lib" @@ "net40"
        | "Helios.NET35" -> "lib" @@ "net35"
        | _ -> "lib" @@ "net45"

    // selected nuget description
    let description project =
        match project with
        | _ -> description

     // add Helios dependency for other projects
    let getHeliosDependency project =
        match project with
        | "Helios" -> []
        | "Helios.NET40" -> []
        | "Helios.NET35" -> []
        | _ -> ["Helios", release.NugetVersion]

open Nuget

//--------------------------------------------------------------------------------
// Clean nuget directory
Target "CleanNuget" (fun _ ->
    CleanDir nugetDir
)

//--------------------------------------------------------------------------------
// Pack nuget for all projects
// Publish to nuget.org if nugetkey is specified

let createNugetPackages _ =
    let mutable dirName = 1
    let removeDir dir = 
        let del _ = 
            DeleteDir dir
            not (directoryExists dir)
        runWithRetries del 3 |> ignore

    let getDirName workingDir dirCount =
        workingDir + dirCount.ToString()

    CleanDir workingDir

    ensureDirectory nugetDir
    for nuspec in !! "src/**/*.nuspec" do
        let ourWorkingDir = getDirName workingDir dirName
        printfn "Creating nuget packages for %s" nuspec
        
        ensureDirectory ourWorkingDir

        let project = Path.GetFileNameWithoutExtension nuspec 
        let projectDir = Path.GetDirectoryName nuspec
        let projectFile = (!! (projectDir @@ project + ".*sproj")) |> Seq.head
        
        
       
        let packages = projectDir @@ "packages.config"        
        let packageDependencies = if (fileExists packages) then (getDependencies packages) else []
        let dependencies = packageDependencies @ getHeliosDependency project
        let releaseVersion = release.NugetVersion

        let pack outputDir symbolPackage =
            NuGetHelper.NuGet
                (fun p ->
                    { p with
                        Description = description project
                        Authors = authors
                        Copyright = copyright
                        Project =  project
                        Properties = ["Configuration", "Release"]
                        ReleaseNotes = release.Notes |> String.concat "\n"
                        Version = releaseVersion
                        Tags = tags |> String.concat " "
                        OutputPath = outputDir
                        WorkingDir = ourWorkingDir
                        SymbolPackage = symbolPackage
                        Dependencies = dependencies })
                nuspec

        // Copy dll, pdb and xml to libdir = workingDir/lib/net45/
        for bin in getProjectBinaries project do
            printfn "Handling nuget for %s" bin
            let releaseDir = "src" @@ bin @@ "bin" @@ "Release"
            printfn "Copying binaries from %s" releaseDir
            let libDir = ourWorkingDir @@ getProjectBinFolders bin
            printfn "Creating output directory %s" libDir
            ensureDirectory libDir
            !! (releaseDir @@ bin + ".dll")
            ++ (releaseDir @@ bin + ".pdb")
            ++ (releaseDir @@ bin + ".xml")
            |> CopyFiles libDir

        // Copy all src-files (.cs and .fs files) to workingDir/src
        let nugetSrcDir = ourWorkingDir @@ @"src/"
        // CreateDir nugetSrcDir

        let isCs = hasExt ".cs"
        let isFs = hasExt ".fs"
        let isAssemblyInfo f = (filename f).Contains("AssemblyInfo")
        let isSrc f = (isCs f || isFs f) && not (isAssemblyInfo f) 
        CopyDir nugetSrcDir projectDir isSrc
        
        //Remove workingDir/src/obj and workingDir/src/bin
        removeDir (nugetSrcDir @@ "obj")
        removeDir (nugetSrcDir @@ "bin")

        // Create both normal nuget package and symbols nuget package. 
        // Uses the files we copied to workingDir and outputs to nugetdir
        pack nugetDir NugetSymbolPackage.Nuspec
        
        //removeDir ourWorkingDir
        dirName <- dirName + 1

let publishNugetPackages _ = 
    let rec publishPackage url accessKey trialsLeft packageFile =
        let tracing = enableProcessTracing
        enableProcessTracing <- false
        let args p =
            match p with
            | (pack, key, "") -> sprintf "push \"%s\" %s" pack key
            | (pack, key, url) -> sprintf "push \"%s\" %s -source %s" pack key url

        tracefn "Pushing %s Attempts left: %d" (FullName packageFile) trialsLeft
        try 
            let result = ExecProcess (fun info -> 
                    info.FileName <- nugetExe
                    info.WorkingDirectory <- (Path.GetDirectoryName (FullName packageFile))
                    info.Arguments <- args (packageFile, accessKey,url)) (System.TimeSpan.FromMinutes 1.0)
            enableProcessTracing <- tracing
            if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe (args (packageFile, "key omitted",url))
        with exn -> 
            if (trialsLeft > 0) then (publishPackage url accessKey (trialsLeft-1) packageFile)
            else raise exn
    let shouldPushNugetPackages = hasBuildParam "nugetkey"
    let shouldPushSymbolsPackages = (hasBuildParam "symbolspublishurl") && (hasBuildParam "symbolskey")
    
    if (shouldPushNugetPackages || shouldPushSymbolsPackages) then
        printfn "Pushing nuget packages"
        if shouldPushNugetPackages then
            let normalPackages= 
                !! (nugetDir @@ "*.nupkg") 
                -- (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in normalPackages do
                publishPackage (getBuildParamOrDefault "nugetpublishurl" "") (getBuildParam "nugetkey") 3 package

        if shouldPushSymbolsPackages then
            let symbolPackages= !! (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in symbolPackages do
                publishPackage (getBuildParam "symbolspublishurl") (getBuildParam "symbolskey") 3 package

Target "Nuget" <| fun _ -> 
    createNugetPackages()
    publishNugetPackages()

Target "CreateNuget" <| fun _ -> 
    createNugetPackages()

Target "PublishNuget" <| fun _ -> 
    publishNugetPackages()

//--------------------------------------------------------------------------------
// Help 
//--------------------------------------------------------------------------------

Target "Help" <| fun _ ->
    List.iter printfn [
      "usage:"
      "build [target]"
      ""
      " Targets for building:"
      " * Build      Builds"
      " * Nuget      Create and optionally publish nugets packages"
      " * RunTests   Runs tests"
      " * All        Builds, run tests, creates and optionally publish nuget packages"
      ""
      " Other Targets"
      " * Help       Display this help" 
      " * HelpNuget  Display help about creating and pushing nuget packages" 
      ""]

Target "HelpNuget" <| fun _ ->
    List.iter printfn [
      "usage: "
      "build Nuget [nugetkey=<key> [nugetpublishurl=<url>]] "
      "            [symbolskey=<key> symbolspublishurl=<url>] "
      "            [nugetprerelease=<prefix>]"
      ""
      "Arguments for Nuget target:"
      "   nugetprerelease=<prefix>   Creates a pre-release package."
      "                              The version will be version-prefix<date>"
      "                              Example: nugetprerelease=dev =>"
      "                                       0.6.3-dev1408191917"
      ""
      "In order to publish a nuget package, keys must be specified."
      "If a key is not specified the nuget packages will only be created on disk"
      "After a build you can find them in bin/nuget"
      ""
      "For pushing nuget packages to nuget.org and symbols to symbolsource.org"
      "you need to specify nugetkey=<key>"
      "   build Nuget nugetKey=<key for nuget.org>"
      ""
      "For pushing the ordinary nuget packages to another place than nuget.org specify the url"
      "  nugetkey=<key>  nugetpublishurl=<url>  "
      ""
      "For pushing symbols packages specify:"
      "  symbolskey=<key>  symbolspublishurl=<url> "
      ""
      "Examples:"
      "  build Nuget                      Build nuget packages to the bin/nuget folder"
      ""
      "  build Nuget nugetprerelease=dev  Build pre-release nuget packages"
      ""
      "  build Nuget nugetkey=123         Build and publish to nuget.org and symbolsource.org"
      ""
      "  build Nuget nugetprerelease=dev nugetkey=123 nugetpublishurl=http://abc"
      "              symbolskey=456 symbolspublishurl=http://xyz"
      "                                   Build and publish pre-release nuget packages to http://abc"
      "                                   and symbols packages to http://xyz"
      ""]


//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "Mono" DoNothing
Target "All" DoNothing

// build dependencies
"Clean" ==> "AssemblyInfo" ==> "RestorePackages" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// tests dependencies
"CleanTests" ==> "RunTests"
"CleanNuget" ==> "BuildRelease" ==> "Nuget"

// nuget dependencies

"BuildRelease" ==> "All"
"RunTests" ==> "All"
"Nuget" ==> "All"

Target "AllTests" DoNothing //used for Mono builds, due to Mono 4.0 bug with FAKE / NuGet https://github.com/fsharp/fsharp/issues/427
"BuildRelease" ==> "AllTests"
"RunTests" ==> "AllTests"

RunTargetOrDefault "Help"