# Dynamic To-Do List Mod for Valheim

This is a Valheim mod that adds a dynamic [To-Do List](ValheimModToDo/README.md) to Valheim that tracks the resources you have and still need to completed crafting or building recipes on the list.

The To-Do List shows the list of to-do recipes and what resources are required. The panel also shows how many of the each resource is needed in total, how many you have in your inventory now and if you need to collect more of them.

Shoutout to game Satisfactory for inspirations.

Download the releases from [To-Do List Thunderstore](https://thunderstore.io/c/valheim/p/Iskindur/ToDoList/).


# To-Do

Prio:

 - When Skills-list open -> Hide To-Do List temporarily

Next:

 - Allow tracking a container (e.g. ship) inventory instead of just player inventory (filling a ship with proper set of items)
 - Add "Show To-Do List" button to Inventory?
 - Add "Hide To-Do List" toggle in Edit Mode
 - Add way to remove and add recipes and their amounts from/in the list (in Edit Mode)
 - Add way to "Include Container Items"-mode option when showing needed resources (Requires: Container Tracking)
 - Add crafting recipe with Insert-key
 - Allow making recipe presets (e.g. Ahjo + Työpöytä + ...)
 - Configuration options:
	+ Keybindings
 - Translations to texts not in game already


# Reverse Engineering

```c#
Container selectedContainer;

// Change text data object value?
selectedContainer.m_nview.GetZDO().Set(ZDOVars.s_text, text); // text = new text value

// Get text data object value?
selectedContainer.m_nview.GetZDO().GetString(ZDOVars.s_text, ""); // "" = default
```


# Background Material

Interesting readings:

 - [Craft All Button Mod](https://github.com/fiote/valheim-craftall/tree/master)
 - [Getting Started by RandyKnapp](https://github.com/RandyKnapp/ValheimMods/blob/main/ValheimModding-GettingStarted.md)
 - [UI Tweaks Mod Source](https://thunderstore.io/c/valheim/p/shudnal/MyLittleUI/source/)
