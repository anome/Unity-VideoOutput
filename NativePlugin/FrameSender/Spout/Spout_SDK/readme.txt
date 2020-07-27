SPOUT

=============
Release 2.006
=============

The Spout 2.006 installation includes "SpoutSettings.exe" which allows selection of :

o DirectX 9 or DirectX 11 functions for texture sharing
o OpenGL pixel buffers for fast transfer of texture data to/from the CPU
o Sharing mode :
  - Texture sharing by way of the OpenGL/NVIDIA interop (preferred)
  - Texture sharing by way of CPU - for systems that do not support the interop
  - Memory sharing where DirectX texture sharing is not supported.
o Selection of preferred GPU for systems with Optimus graphics.
o Setting the maximum number of simultaneous Spout senders.

The selected sharing mode applies to all Spout applications that are built with the 2.006 SDK which means that the developer does not need to build a project in anticipation of compatibility, the user can select and the application will adapt.

Contact us at spout.zeal.co

- - - - - - - - - - - - - -
Updates since 2.006 initial release

16.04.17 - rebuild Spout Freeframe dlls VS2012 with original IDs for Isadora
31.10.17 - SpoutReceiver2.dll
         - close receiver on receivetexture fail
           https://github.com/leadedge/Spout2/issues/25
           Version 3.031
18.11.17 - Spout demo receiver - fullscreen for multiple monitors thanks to Tim Thompson - Version 2.020
         - Change Visual Studio redistributable installation to 32bit only
           https://github.com/leadedge/Spout2/issues/31
01.03.18 - SpoutPanel : read MaxSenders registry entry
      	   https://github.com/leadedge/Spout2/issues/33
         - Version 2.22
02.03.18 - Add MaxSenders registry entry
         - Replace SpoutDXmode with SpoutSettings to allow setting maximum senders.
	 - Update demo programs : sender increment the sender name if the same one is started
         - Sender Version 2.020
12.11.18 - Rebuild Milkdrop plugin to fix DirectX device release memory leak
         - Update Spout SDK source to align with GitHub

