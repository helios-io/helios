$: << './'
require "rubygems"
require "bundler"
Bundler.setup

require 'albacore'
require 'version_bumper'

#-----------------------
# Local dependencies
#-----------------------
require File.expand_path(File.dirname(__FILE__)) + '/buildscripts/projects'
require File.expand_path(File.dirname(__FILE__)) + '/buildscripts/paths'
require File.expand_path(File.dirname(__FILE__)) + '/buildscripts/nuspec'

#-----------------------
# Environment variables
#-----------------------
@env_buildconfigname = "Release"

def env_buildversion
    bumper_version.to_s
end

def env_nuget_version
    version = env_buildversion.split(".")
    "#{version[0]}.#{version[1]}.#{version[2]}.#{version[3]}"
end

#-----------------------
# Control Flow (meant to be called directly)
#-----------------------

desc "Creates a new Release build of helios locally"
task :default => [:build, :nunit_tests]

desc "Creates a new Debug build of helios locally"
task :debug => [:set_debug_config, :default]

desc "Packs a Release build of helios for NuGet"
task :nuget => [:default, :pack, :pack_symbol]

desc "Packs a Debug build of helios for NuGet"
task :nuget_debug => [:debug, :pack, :pack_symbol]

#-----------------------
# Environment variables
#-----------------------
desc "Sets the build environment to Debug"
task :set_debug_config do
    @env_buildconfigname = "Debug"
end

#-----------------------
# MSBuild
#-----------------------

desc "Does a release build of everything in the solution"
msbuild :build => [:assemblyinfo] do |msb|
    msb.properties :configuration => @env_buildconfigname
    msb.targets :Clean, :Build #Does the equivalent of a "Rebuild Solution"
    msb.solution = File.join(Folders[:root], Files[:solution])
end

#-----------------------
# Version Management
#-----------------------

assemblyinfo :assemblyinfo do |asm|
    assemblyInfoPath = File.join(Folders[:src], Files[:assembly_info])

    asm.input_file = assemblyInfoPath
    asm.output_file = assemblyInfoPath

    asm.version = env_buildversion
    asm.file_version = env_buildversion
end

desc "Increments the build number for the project"
task :bump_build_number do
    bumper_version.bump_build
    bumper_version.write(File.join(Folders[:root], Files[:version]))
end

desc "Increments the revision number for the project"
task :bump_revision_number do
    bumper_version.bump_revision
    bumper_version.write(File.join(Folders[:root], Files[:version]))
end

desc "Increments the minor version number for the project"
task :bump_minor_version_number do
    bumper_version.bump_minor
    bumper_version.write(File.join(Folders[:root], Files[:version]))
end

desc "Increments the major version number for the project"
task :bump_major_version_number do
    bumper_version.bump_major
    bumper_version.write(File.join(Folders[:root], Files[:version]))
end

#-----------------------
# Output
#-----------------------
desc "Sets the output / bin folders based on the current build configuration"
task :set_output_folders do
    #.NET 4.5
    Folders[:bin][:helios_net45] = File.join(Folders[:src], Projects[:helios_net45][:dir],"bin", @env_buildconfigname)

    # .NET 4.0
    Folders[:bin][:helios_net40] = File.join(Folders[:src], Projects[:helios_net40][:dir],"bin", @env_buildconfigname)

    # .NET 3.5
    Folders[:bin][:helios_net35] = File.join(Folders[:src], Projects[:helios_net35][:dir],"bin", @env_buildconfigname)
    Folders[:bin][:helios_net45_tests] = File.join(Folders[:tests], Projects[:helios_net45][:tests],"bin", @env_buildconfigname)
end

desc "Wipes out the build folder so we have a clean slate to work with"
task :clean_output_folders => :set_output_folders do
    puts "Flushing build folder..."
    flush_dir(Folders[:nuget_out])
end

desc "Creates all of the output folders we need for ILMerge / NuGet"
task :create_output_folders => :clean_output_folders do
    create_dir(Folders[:out])

    #Nuget folders
    create_dir(Folders[:nuget_out])
    create_dir(Folders[:helios_nuspec][:root])
    create_dir(Folders[:helios_nuspec][:lib])
    create_dir(Folders[:helios_nuspec][:net40])
    create_dir(Folders[:helios_nuspec][:net45])

    create_dir(Folders[:helios_net35_nuspec][:root])
    create_dir(Folders[:helios_net35_nuspec][:lib])
    create_dir(Folders[:helios_net35_nuspec][:net35])

    create_dir(Folders[:helios_symbol_nuspec][:root])
    create_dir(Folders[:helios_symbol_nuspec][:lib])
    create_dir(Folders[:helios_symbol_nuspec][:src])
    create_dir(Folders[:helios_symbol_nuspec][:net45])
    create_dir(Folders[:helios_symbol_nuspec][:net40])
end

#-----------------------
# NuGet Output
#-----------------------
output :helios_net45_nuget_output => [:create_output_folders] do |out|
    out.from Folders[:bin][:helios_net45]
    out.to Folders[:helios_nuspec][:net45]
    out.file Files[:helios_net45][:bin]
end

output :helios_net40_nuget_output => [:create_output_folders] do |out|
    out.from Folders[:bin][:helios_net40]
    out.to Folders[:helios_nuspec][:net40]
    out.file Files[:helios_net40][:bin]
end

output :helios_symbol_nuget_output => [:create_output_folders] do |out|
    out.from Folders[:bin][:helios_net45]
    out.to Folders[:helios_symbol_nuspec][:net45]
    out.file Files[:helios_net45][:bin]
    out.file Files[:helios_net45][:pdb]
end

output :helios_symbol_net40_nuget_output => [:create_output_folders] do |out|
    out.from Folders[:bin][:helios_net40]
    out.to Folders[:helios_symbol_nuspec][:net40]
    out.file Files[:helios_net40][:bin]
    out.file Files[:helios_net40][:pdb]
end

output :helios_net35_nuget_output => [:create_output_folders] do |out|
    out.from Folders[:bin][:helios_net35]
    out.to Folders[:helios_net35_nuspec][:net35]
    out.file Files[:helios_net35][:bin]
end

task :helios_symbol_src_nuget_output => [:create_output_folders] do |out|
    src = File.join(Folders[:src], Projects[:helios_net45][:dir])
    dest = Folders[:helios_symbol_nuspec][:src]
    FileUtils.cp_r Dir.glob(src + '/*.cs'), dest
    FileUtils.cp_r File.join(src, "Buffers"), dest
    FileUtils.cp_r File.join(src, "Concurrency"), dest
    FileUtils.cp_r File.join(src, "Eventing"), dest
    FileUtils.cp_r File.join(src, "Exceptions"), dest
    FileUtils.cp_r File.join(src, "Net"), dest
    FileUtils.cp_r File.join(src, "Monitoring"), dest
    FileUtils.cp_r File.join(src, "Ops"), dest
    FileUtils.cp_r File.join(src, "Properties"), dest
    FileUtils.cp_r File.join(src, "Reactor"), dest
    FileUtils.cp_r File.join(src, "Serialization"), dest
    FileUtils.cp_r File.join(src, "Topology"), dest
    FileUtils.cp_r File.join(src, "Util"), dest
end

desc "Executes all file/copy tasks"
task :all_output => [:helios_net45_nuget_output, 
    :helios_net40_nuget_output,
    :helios_symbol_net40_nuget_output,
    :helios_symbol_nuget_output, 
    :helios_symbol_src_nuget_output, 
    :helios_net35_nuget_output]

#-----------------------
# NuSpec
#-----------------------
desc "Builds a nuspec file for Helios"
nuspec :nuspec_net45 => [:all_output] do |nuspec|
    nuspec.id = Projects[:helios_net45][:id]
    nuspec.title = Projects[:helios_net45][:title]
    nuspec.version = env_nuget_version
    nuspec.authors = Projects[:helios_net45][:authors]
    nuspec.owners = Projects[:helios_net45][:company]
    nuspec.description = Projects[:helios_net45][:description]
    #nuspec.iconUrl = Projects[:iconUrl]
    nuspec.projectUrl = Projects[:projectUrl]
    nuspec.licenseUrl = Projects[:licenseUrl]
    #nuspec.require_license_acceptance = false #causes an issue with Albacore 0.3.5
    nuspec.language = Projects[:language]
    nuspec.tags = Projects[:helios_net45][:nuget_tags]
    nuspec.output_file = File.join(Folders[:nuget_out], "#{Projects[:helios_net45][:id]}-v#{env_nuget_version}(#{@env_buildconfigname}).nuspec");
end

desc "Builds a nuspec file for helios"
nuspec :nuspec_net35 => [:all_output] do |nuspec|
    nuspec.id = Projects[:helios_net35][:id]
    nuspec.title = Projects[:helios_net35][:title]
    nuspec.version = env_nuget_version
    nuspec.authors = Projects[:helios_net35][:authors]
    nuspec.owners = Projects[:helios_net35][:company]
    nuspec.description = Projects[:helios_net35][:description]
    #nuspec.iconUrl = Projects[:iconUrl]
    nuspec.projectUrl = Projects[:projectUrl]
    nuspec.licenseUrl = Projects[:licenseUrl]
    #nuspec.require_license_acceptance = false #causes an issue with Albacore 0.3.5
    nuspec.language = Projects[:language]
    nuspec.tags = Projects[:helios_net35][:nuget_tags]
    nuspec.output_file = File.join(Folders[:nuget_out], "#{Projects[:helios_net35][:id]}-v#{env_nuget_version}(#{@env_buildconfigname}).nuspec");
    nuspec.dependency "TaskParallelLibrary", "1.0.2856.0"
end

task :nuspec => [:nuspec_net45, :nuspec_net35]

#-----------------------
# NuGet Pack
#-----------------------
desc "Packs a build of Helios into a NuGet package"
nugetpack :pack_net45 => [:nuspec] do |nuget|
    nuget.command = Commands[:nuget]
    nuget.nuspec = File.join(Folders[:nuget_out], "#{Projects[:helios_net45][:id]}-v#{env_nuget_version}(#{@env_buildconfigname}).nuspec")
    nuget.base_folder = Folders[:helios_nuspec][:root]
    nuget.output = Folders[:nuget_out]
end

desc "Packs a build of Helios.NET35 into a NuGet package"
nugetpack :pack_net35 => [:nuspec] do |nuget|
    nuget.command = Commands[:nuget]
    nuget.nuspec = File.join(Folders[:nuget_out], "#{Projects[:helios_net35][:id]}-v#{env_nuget_version}(#{@env_buildconfigname}).nuspec")
    nuget.base_folder = Folders[:helios_net35_nuspec][:root]
    nuget.output = Folders[:nuget_out]
end

desc "Packs a symbol build of helios into a NuGet package"
nugetpack :pack_symbol => [:nuspec] do |nuget|
    nuget.command = Commands[:nuget]
    nuget.nuspec = File.join(Folders[:nuget_out], "#{Projects[:helios_net45][:id]}-v#{env_nuget_version}(#{@env_buildconfigname}).nuspec")
    nuget.base_folder = Folders[:helios_symbol_nuspec][:root]
    nuget.output = Folders[:nuget_out]
    nuget.symbols = true
end

task :pack => [:pack_net45, :pack_net35]

#-----------------------
# NUnit Tests
#-----------------------
nunit :nunit_tests => [:build, :create_output_folders] do |nunit|
    nunit.command = Commands[:nunit]
    nunit.options '/framework v4.0.30319'

    nunit.assemblies File.join(Folders[:bin][:helios_net45_tests], Files[:helios_net45][:tests])
end