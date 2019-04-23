## Unity Video Sender

Unity Video Sender is a plugin for Unity that allows sharing rendered frames with other Applications.

[DOWNLOAD THE PLUGIN HERE](https://github.com/anome/Unity-plugin/releases)

## Motivation

This project aims to gather all techniques of frame video sharing for Unity in a single plugin. Its main goal is to bring all advances from the community in a common plugin that can be maintainable in the long term.

## Features

* Send frames with : NDI, Spout, Syphon (OpenGL Core), Syphon (Metal)
* Send frames in Play mode and in Edit mode
* Switch between techniques with a single click


## System Requirements
* Unity 2018.3 or above
* Windows or OSX, 64-bit only

## Installation

Download and import one of the .unitypackage files from [Releases page](https://github.com/anome/Unity-plugin/releases).

## Developers corner

### Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

### Technical Overview

The code is split in two parts :

* A C++/ObjC nativePlugin, completely separated from Unity context and built as a .bundle (osx) or .dll (win)
* A C# editorPlugin running in the Unity Editor context and built as a managed .dll

* We are using a custom Syphon Framework that works with Metal and allows to create custom server implementations.  
The fork is available here [Syphon Framework Fork](https://github.com/anome/Syphon-Framework/tree/custom-servers)


### How To build

#### Software Stack
- For OSX and Windows, [Visual Studio](https://visualstudio.microsoft.com/fr/vs/community/)
- [Unity](https://unity3d.com/)
- For OSX, [Xcode 10](https://developer.apple.com/xcode/)
- You might need to manually download the [.NET Framework](https://dotnet.microsoft.com/)

#### Build instructions
- Open EditorPlugin/UVSEditor.sln and run the build
- Open the Xcode project (OSX) or VisualStudio project (Windows) inside the NativePlugin folder and run the build

#### Built files
- The built files are available in the "dist" folder
- Built files are automatically copied to the "DemoUnityProject" for test purposes
- Don't forget : it is mandatory to restart Unity when updating the Native Plugin DLL (UVSNative)

### Known Issues
- Syphon OpenGL might loose the texture in Edit mode while switching between the Game and Scene view
- The linear color space and changes between linear/gamma color space are not properly supported
- Some exotic resolutions can cause problems with NDI (For example 499x312)

## Credits
This plugin was made possible thanks to : 
* The folks behind [Syphon](https://github.com/Syphon/Syphon-Framework/)
* The folks behind [Spout](https://github.com/leadedge/Spout2)
* The folks behind [NewTech NDI](https://www.newtek.com/ndi/)
* [Keijiro Takahashi](https://github.com/keijiro/) for his work of bringing all of these technologies in Unity

## License
See the licence file for more details
