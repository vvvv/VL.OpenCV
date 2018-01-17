# VL.OpenCV
A [VL](https://vvvv.org/documentation/vl) wrapper for [OpenCVSharp3-AnyCPU](https://github.com/shimat/opencvsharp)

## Using
In order to use this library with vl you'll have to install the nuget that will be available on nuget.org

See ... on how to install and use nugets.

## Developing
If you want to contribute to this repository, clone it into a directory like:
 
    X:\vl-libs\VL.OpenCV
 
then start vvvv with the commandline parameter:

    /package-repositories "X:\vl-libs\"
    
which will make all packs found in that directory available as dependencies in vl documents. Note that it is possible to have both the nuget and the sources available. If both are found, the one in the "package-repositories" path is used. Like this you can easily switch between your local development version and the nuget by simply including the local version in that search path or not.

Then open

    \src\VL.OpenCV.sln
    
in VisualStudio and build it. This is necessary for a few things that cannot yet be expressed in vl directly, like dynamic enums and static readonly instances of things. 
