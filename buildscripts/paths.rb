#----------------------------------
# Paths and file system functions for Helios
#----------------------------------
root_folder = File.expand_path("#{File.dirname(__FILE__)}/..")

Folders = {
    :root => root_folder,
    :src => File.join(root_folder, "src"),
    :tests => File.join(root_folder, "tests"),
    :out => File.join(root_folder, "build"),
    :tools => File.join(root_folder,"tools"),
    :nuget_bin => File.join(root_folder, ".nuget"),
    :nuget_out => File.join(root_folder, "build", "nuget"),
    :nunit => File.join(root_folder, "tools", "nunit"),

    #Output folder for creating Helios nuget distributions
    :helios_nuspec => {
        :root => File.join(root_folder, "build", "nuget", "Helios"),
        :lib => File.join(root_folder, "build", "nuget", "Helios", "lib"),
        :net40 => File.join(root_folder, "build", "nuget", "Helios", "lib", "net40"),
        :net45 => File.join(root_folder, "build", "nuget", "Helios", "lib", "net45"),
    },

    :helios_net35_nuspec => {
        :root => File.join(root_folder, "build", "nuget", "HeliosNET35"),
        :lib => File.join(root_folder, "build", "nuget", "HeliosNET35", "lib"),
        :net35 => File.join(root_folder, "build", "nuget", "HeliosNET35", "lib", "net35"),
    },

    :helios_symbol_nuspec => {
        :root => File.join(root_folder, "build", "nuget", "Helios-Symbol"),
        :lib => File.join(root_folder, "build", "nuget", "Helios-Symbol", "lib"),
        :src => File.join(root_folder, "build", "nuget", "Helios-Symbol", "src"),
        :net35 => File.join(root_folder, "build", "nuget", "Helios-Symbol", "lib", "net35"),
        :net45 => File.join(root_folder, "build", "nuget", "Helios-Symbol", "lib", "net45"),
    },

    #specifies the locations of the binary DLLs we want to use in NuGet / NUnit
    :bin => {
        :helios_net45 => 'placeholder - specify build environment',
        :helios_net40 => 'placeholder - specify build environment',
        :helios_net35 => 'placeholder - specify build environment',
        :helios_net45_tests => 'placeholder - specify build environment'
    }
}

Files = {
    :solution => "Helios.sln",
    :version => "VERSION",
    :assembly_info => "SharedAssemblyInfo.cs",

    :helios_net45 => {
        :bin => "#{Projects[:helios_net45][:id]}.dll",
        :pdb => "#{Projects[:helios_net45][:id]}.pdb",
        :tests => "#{Projects[:helios_net45][:tests]}.dll",
    },

    :helios_net40 => {
        :bin => "#{Projects[:helios_net40][:id]}.dll",
        :pdb => "#{Projects[:helios_net40][:id]}.pdb",
    },

    :helios_net35 => {
        :bin => "#{Projects[:helios_net35][:id]}.dll",
        :pdb => "#{Projects[:helios_net35][:id]}.pdb",
    }
}

Commands = {
    :nunit => File.join(Folders[:nunit], "nunit-console.exe"),
    :nuget => File.join(Folders[:nuget_bin], "NuGet.exe"),
}

#safe function for creating output directories
def create_dir(dirName)
    if !File.directory?(dirName)
        FileUtils.mkdir(dirName) #creates the /build directory
    end
end

#Deletes a directory from the tree (to keep the build folder clean)
def flush_dir(dirName)
    if File.directory?(dirName)
        FileUtils.remove_dir(dirName, true)
    end
end