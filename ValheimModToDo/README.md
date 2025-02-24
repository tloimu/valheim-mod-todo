# Dynamic To-Do List Mod for Valheim

Adds a dynamic To-Do List to Valheim that tracks the resources you have and still need to completed crafting or building recipes on the list.

The To-Do List shows the list of to-do recipes and what resources are required. The panel also shows how many of the each resource is needed in total, how many you have in your inventory now and if you need to collect more of them.

Shoutout to game Satisfactory for inspirations.


## Features

Show/hide the *To-Do List* pressing `<Home>` key. Toggling the panel to visible will also force update it in case something is not in sync with your inventory and the to-do list panel.

Add the selected crafting recipe to the list from crafting panel by pressing `<Insert>` key or by clicking the new **Add to To-Do** button.

Add the building recipe to the list by pressing `<Insert>` key while placing a building piece.

Change panel to *Edit Mode* by pressing `<PgUp>` key. In Edit Mode, you can edit the To-Do List in more detail in a separate panel that appears.

Clear the list by clicking the **Clear Crafting To-Do list** button on the To-Do List panel when in Edit Mode.

The needed amount of resources is automatically updated when you:

 - pick an item to your inventory
 - drop an item from your inventory
 - move an item between your inventory and a container
 - complete crafting or building an recipe item on the list

Completing crafting or building an recipe item on the list will also automatically remove it from the list.

The To-Do List is stored in a file `todo-list-for-{PlayerName}-in-{WorldName}-v1.xml` in Valheim's data folder.


## Known issues

Reasons not to use this mod:

 - Missing translations on "Add to To-Do"-button, "Clear Crafting To-Do list"-button and "To-Do" panel title
