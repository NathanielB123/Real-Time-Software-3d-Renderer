# Real-Time-Software-3d-Renderer

A program I created for my EPQ "Creating a Real-time Software 3d Renderer in C#".
It is built entirely in C# and runs with no GPU acceleration (apart from the displaying of the final frame handled by WinForms) and supports quite a few modern rendering features including:
* Diffuse and specular shading
* Texture mapping and filtering
* Real time shadows
* Screen space and cube map reflections
* Parallax mapping
* Post-process effects such as depth blur (as well as a fake AA and bloom effect)
* It can load .obj files for meshes and .png s for textures.

I have included a few demo scenes that will open when starting the program that showcase these features (ensure the required files are stored in the same directory as the executable in the proper file structure - see "bin" folder.)

There are some pretty major performance issues and limitations but I still think it's pretty cool I managed to get it working as well as it does.
