# Preparation
## Add mono repository [Mono Install Linux](http://www.mono-project.com/docs/getting-started/install/linux/).
* sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
* echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
* sudo apt-get update

## Install Minimal Mono
* sudo apt-get install mono-devel ca-certificates-mono

## Install Complete Mono
* sudo apt-get install mono-complete

## Install MonoDevelop (Optional)
* sudo apt-get install monodevelop

# Build (without MonoDevelop)
Run *./build.sh command [execute]*.
Avaialbe Commands:
* debug
  * builds debug version
* release
  * builds release version 
* releaseplgx
  * builds release with PLGX plugin
  * asks for version number before build

In folder *build* you now see two subfolders:

* bin: compiled output files
* dist: packages for distribution

# Prepare new release
This projects follows [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/). 
The *master* branch contains the latest released version. Each released version is tagged with a version tag: this tag follows [Semantic Versioning](http://semver.org/). Ongoing development takes place in *develop* branch.
When preparing for a new release the last steps before merging *develop* to *master* is to change the version informations (e.g. 0.1.0-alpha):

* KeeAnywhere\Properties\AssemblyInfo.cs
* version_manifest.txt (change only for production releases)

After merging *develop* to *master* change version in *develop* to **next** unstable version (e. g. 0.2.0-unstable) - just to make clear ths is a development snapshot and generally not released to public.

# Version examples
Versions are always counted upwards. Each version is unique. 
Additional informations like *alpha*, *beta* only describe the characteristics of the version. 
Until version 1.0.0 the minor version will be increased.
  
* 0.1.0-alpha: Alpha release for previewing or testing new features. Not for production use.
* 0.2.0-unstable: Current development snapshot. These versions will never be released to public.
* 0.2.0-beta: Beta releases are feature complete. Only bugfixing should take place before a new main release
* 1.0.0: production version

 
