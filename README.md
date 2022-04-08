# VL.OpenCV
A [VL](https://vvvv.org/documentation/vl) wrapper for [OpenCV](https://opencv.org) based on [OpenCvSharp](https://github.com/shimat/opencvsharp) and also using [SharpDX MediaFoundation](http://sharpdx.org/wiki/class-library-api/mediafoundation/).

Try it with vvvv, the visual live-programming environment for .NET  
Download: http://visualprogramming.net

## Using the library
In order to use this library with vl you have to install the nuget that is available via nuget.org. For information on how to use nugets with vl, see [Manage Nugets](https://thegraybook.vvvv.org/reference/libraries/dependencies.html#manage-nugets) in the vl documentation. As described there you go to the commandline and then type:

    nuget install VL.OpenCV

For pre-release versions use this instead:

	nuget install VL.OpenCV -pre

Once the VL.OpenCV nuget is installed and referenced in your vl document you'll see the category "OpenCV" in the nodebrowser. From there explore the nodes in its main sub-categories:

- Source (VideoIn, ImageReader,...)
- Sink (Renderer, ImageWriter)
- Filter (Blur, Dilate, Sobel,...)
- Detection (Contours, ObjectDetector, MarkerDetector,...)

Demo patches can be found using the Help Browser, or navigating here:

    "\lib\packs\VL.OpenCV...\help\"

And vvvv beta demo patches are somewhat hidden here:

    "\lib\packs\VL.OpenCV...\vvvv\girlpower\"

## Contributing to the development
If you want to contribute to this repository, clone it into a directory like:
 
    X:\vl-libs\VL.OpenCV

### Build the C# Project
Open

    X:\vl-libs\VL.OpenCV\src\VL.OpenCV.sln
    
in VisualStudio 2019 and build it. This is necessary for a few things that cannot yet be expressed in VL directly, like dynamic enums and static readonly instances of things. 

### Get Nuget Dependency
This wrapper is depending on two thirdparty nugets: [OpenCvSharp4.runtime.win](https://github.com/shimat/opencvsharp#installation) and [OpenCvSharp4](https://github.com/shimat/opencvsharp#installation). When installing the VL.OpenCV nuget as mentioned under "Using the library" above, this dependency will be installed automatically. To install it otherwise, go to your vvvv's

    \lib\packs 
    
on a commandline and run

    nuget.exe install OpenCvSharp4.Windows

### Start vvvv
Then start vvvv with the commandline parameter:

    --package-repositories "X:\vl-libs\"
    
which will make all packs found in that directory available as dependencies in vl documents. Note that it is possible to have both the nuget (binary) and the sources available. If both are found, the one in the "package-repositories" path is used. Like this you can easily switch between your local development version and the official nuget by simply including your local version in that search path or not.

VL.OpenCV will now show up as Nuget among a documents dependencies as shown [here](https://thegraybook.vvvv.org/reference/libraries/dependencies.html#nugets).
