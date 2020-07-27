## Unity Video Output

Unity Video Ouput is a plugin for Unity that allows wiring cameras to other Applications using Syphon-Spout and NDI.

[DOWNLOAD THE PLUGIN HERE](https://github.com/anome/Unity-plugin/releases)

## Motivation

This project aims to gather all techniques of frame video sharing for Unity in a single plugin. Its main goal is to bring all advances from the community in a common plugin that can be maintainable in the long term.

## Features

* Share camera using: NDI or Syphon-Spout
* Switch between techniques in one click
* cross-platform (Windows, MacOS) and cross-pipeline (Standalone, HDRP, URP)
* Works in Play mode and in Edit mode (for Edit Mode, game view window must be visible and Unity in focus)


## System Requirements
* Unity 2019.4
* Windows or MacOS, 64-bit only

## Installation

Download and import in Unity one of the .unitypackage from [Releases page](https://github.com/anome/Unity-plugin/releases).

## Known Issues

- Some exotic resolutions can cause problems with NDI (For example 499x312)
- in URP, post process volumes are not taken into account (Unity's fault?)
- The linear color space and changes between linear/gamma color space are not properly supported
- Syphon colorspace can be mixed up between linear and gamma, causing too constrasty or too flat images on the receiver. Workaround: remove line n°18 on SrpMetalBlit.shader (see comment in the source file)
- Something else ? [Tell us by creating an issue](https://github.com/anome/Unity-VideoOutput/issues)


## Developers corner

### Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

### Technical Overview

The code is split in two parts :

* A C++/ObjC nativePlugin, completely separated from Unity context and built as a .bundle (MacOS) or .dll (Windows)
* A C# Unity Package

* We are using a custom Syphon Framework that works with Metal and allows to create custom server implementations.  
The fork is available here [Syphon Framework Fork](https://github.com/anome/Syphon-Framework/tree/custom-servers)


### How To build

#### Software Stack
- For MacOS and Windows, [Visual Studio](https://visualstudio.microsoft.com/fr/vs/community/)
- [Unity](https://unity3d.com/)
- For MacOS, [Xcode 10](https://developer.apple.com/xcode/)
- You might need to manually download the [.NET Framework](https://dotnet.microsoft.com/)

#### Build instructions

- Create a Unity project and load the package as a local package
- Open a script file from your Unity project to access the code in VS Studio
- Code & Save. your Unity project will live-reload


_For the Native plugin:_
- Open the Xcode project or VisualStudio project inside the NativePlugin folder and run the build (make sure to choose 64bit build)
- .dll/bundle files are automatically updated in the package folder Editor/Native
- Don't forget: it is mandatory to restart Unity when updating a Native Plugin DLL



## Credits
This plugin was made possible thanks to : 
* The folks behind [Syphon](https://github.com/Syphon/Syphon-Framework/)
* The folks behind [Spout](https://github.com/leadedge/Spout2)
* The folks behind [NewTech NDI](https://www.newtek.com/ndi/)
* [Keijiro Takahashi](https://github.com/keijiro/) for his work of bringing all of these technologies in Unity

## License
See the licence file for more details
