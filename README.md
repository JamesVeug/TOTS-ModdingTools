# Tales of the Shire Modding Tools

This repository contains all the content used to easily mod Tales of the Shire.
Provides exporting of game files to easily modifythem with a text editor and have the changes show in the game and more.

[Download the Modding Tools mod from Nexus Mods](https://www.nexusmods.com/talesoftheshirealordoftheringsgame/mods/3) 



## [Modding Tools](https://github.com/JamesVeug/TOTS-ModdingTools)

This mod is designed to be used in conjunction with other mods that wish to add new content to the game. 

It provides a number of useful functions for adding new things to the game in a way that removes the struggles of understanding the Tales of the Shire code base and more.

Use this to save yourself HEAPS the pain of understanding the TOTS codebase and why adding stuff doesn't work and giving up in the end. 

[Read the intenal Readme](https://github.com/JamesVeug/TOTS-ModdingTools/blob/main/TOTS_ModdingTools/README.md) for code examples on how to use the API.


## Discord
If you require help with a mod, have ideas you want to share, want to report a problem or want to contribute to the modding scene you can join our discord.

https://discord.gg/dNYuREtRGx

## Build

Before opening the solution, you need to create a file named `Directory.Path.props` in the root of the repository. This file should specify the path to your ATS game executable. Below is an example of how to set it up (the content of this file):
```xml
<Project>
  <PropertyGroup>
    <TOTSPath>C:\Program Files (x86)\Steam\steamapps\common\TOTS</TOTSPath>
  </PropertyGroup>
</Project>
```
Make sure to replace the TOTSPath value with the actual installation path of your game.

