# Dynamic To-Do List Mod for Valheim

# To-Do

 - Project to Git
 - Project to Thunderstore (https://thunderstore.io)
 - Update To-Do view when moving things to/from player inventory from/to a container
 - Add free text notes
 - With Shift+Home -> Open to-do view in block input mode:
	+ show and allow pressing Clear All, Edit Notes, Remove Eecipe etc.
	+ Select from preset of recipes
	+ Add, edit and delete recipes presets
 - Allow making recipe presets (e.g. Ahjo + Työpöytä + ...)
 - Allow tracking a container (e.g. ship) inventory instead of just player inventory (filling a ship with proper set of items)
 - Configuration options:
	+ show todo list as just floating text vs. wood panel
	+ Keybindings
 - Saving to-do list persistently (per map, per character)


# Background Material

Interesting readings:

 - [Craft All Button Mod](https://github.com/fiote/valheim-craftall/tree/master)
 - [Getting Started by RandyKnapp](https://github.com/RandyKnapp/ValheimMods/blob/main/ValheimModding-GettingStarted.md)
 - [UI Tweaks Mod Source](https://thunderstore.io/c/valheim/p/shudnal/MyLittleUI/source/)



 # Data Models And Flows

When player crafts an item into inventory, the following is triggered:

Patch: `InventoryGui.DoCrafting(InventoryGui __instance, Player player)`

The recipe name and requirements are in
```
recipe := __instance.m_selectedRecipe.Recipe
requirements := recipe.m_requirements
name := recipe.name
```

The resources used are in

```
recipe.m_resources => each resource { m_resItem.name, .resItem.m_amount }

quality level ?
```

When an item is built onto world, the following is triggered

Patch: `Player.PlacePiece(Player __instance, Piece piece, Vector3 pos, Quaternion rot, bool doAttack)`

The recipe name and resources are in
```
name := piece.name
resources := piece.m_resources
```

The resources used are in

```
piece.m_resources => each resource { m_resItem.name, .resItem.m_amount }

quality level ?
```


## Item Identifiers

Player inventory returns `GetAllItems()` in `m_name`:

```
$item_wood
$item_deerhide
```

Requirements (Piece and Recipie) have `name`:

```
Wood
Flint
LeatherScraps
...
wood_fence
stake_wall
```

Recipe `name` is like:
```
Recipe_AxeFlint
```

and Recipe `m_item.name` are like:
```
AxeFlint
```


# JötunnModStub

A Valheim mod stub project using [Jötunn](https://github.com/Valheim-Modding/Jotunn) including build tools and a basic Unity project stub.
There is no actual plugin content included, just a minimum plugin class. 

#  Setup Guide

Please see [Jötunn Docs](https://valheim-modding.github.io/Jotunn/guides/overview.html) detailed documentation and setup.

### Post Build automations

Included in this repo is a PowerShell script `publish.ps1`.
The script is referenced in the project file as a post-build event.
Depending on the chosen configuration in Visual Studio the script executes the following actions.

### Building Debug

The compiled dll and a dll.mdb debug file are copied to `<ValheimDir>\BepInEx\plugins` (or the path set in MOD_DEPLOYPATH).

### Building Release

A compressed file with the binaries is created in `<ValheimModToDo>\Packages`ready for upload to ThunderStore.
Dont forget to include your information in the manifest.json and to change the project's readme file.

## Developing Assets with Unity

New Assets can be created with Unity and imported into Valheim using the mod.
A Unity project is included in this repository under `<ValheimModToDo>\JotunnModUnity`.

### Unity Editor Setup

1. [Download](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe) UnityHub directly from Unity or install it with the Visual Studio Installer via `Individual Components` -> `Visual Studio Tools for Unity`
2. You will need an Unity account to register your PC and get a free licence. Create the account, login with it in Unity Hub and get your licence via `Settings` -> `Licence Management`
3. Install Unity Editor version 2022.3.17f
4. Compile the project. This copies all assemblies into `<ValheimModToDo>\JotunnModUnity\Assets\Assemblies`. Don't open Unity yet before this step, it will remove assembly references.
5. **Warning:** These assembly files are copyrighted material and you can theoretically get into trouble when you distribute them in your github repository. To avoid that there is a .gitignore file in the Unity project folder. Keep that when you clone or copy this repository
6. Open Unity Hub and add the JotunnModUnity project
7. Open the project in Unity
8. Install the `AssetBundle Browser` package in the Unity Editor via `Window`-> `Package Manager` for easy bundle creation

## Debugging

See the Wiki page [Debugging Plugins via IDE](https://github.com/Valheim-Modding/Wiki/wiki/Debugging-Plugins-via-IDE) for more information

## Actions after a game update

When Valheim updates it is likely that parts of the assembly files change.
If this is the case, the references to the assembly files must be renewed in Visual Studio and Unity.

### Prebuild actions

1. There is a file called DoPrebuild.props included in the solution. When you set its only value to true, Jötunn will automatically generate publicized assemblies for you. Otherwise you have to do this step manually.

### Unity actions

1. Copy all `assembly_*.dll` from `<ValheimDir>\valheim_Data\Managed` into `<ValheimModToDo>\JotunnModUnity\Assets\Assemblies`. <br />
  **Do this directly in the filesystem - don't import the dlls in Unity**.
2. Go to Unity Editor and press `Ctrl+R`. This reloads all files from the filesystem and "re-imports" the copied dlls into the project.
