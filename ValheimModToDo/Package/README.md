# Dynamic To-Do List Mod for Valheim

Adds a dynamic To-Do List to Valheim that tracks the resources you have and still need to completed crafting or building recipes on the list.

The To-Do List shows the list of to-do recipes and what resources are required. The panel also shows how many of the each resource is needed in total, how many you have in your inventory now and if you need to collect more of them.

Shoutout to game Satisfactory for inspirations.


## Features

Show/hide the *To-Do List* pressing `<Home>` key. Toggling the panel to visible will also force update it in case something is not in sync with your inventory and the to-do list panel.

Add the selected crafting recipe to the list from the inventory UI by clicking the new **Add to To-Do** button or by pressing `<Insert>` key. This works also for item upgrades.

Add the building recipe to the list by pressing `<Insert>` key while placing a building piece.

The To-Do List is swiched to *Edit Mode* when player inventory is open. In Edit Mode, you can edit the To-Do List in more detail in the separate panel that appears.

Clear the list from recipies by clicking the **Clear All** button on the To-Do List panel when in Edit Mode. Each recipe row also has its own increment (+) and decrement (-) amount button to allow manual editing of the list.

The needed amount of resources is automatically updated when you:

 - pick an item to your inventory
 - drop an item from your inventory
 - move an item between your inventory and a container
 - complete crafting or building an recipe item on the list
 - respawn

Completing crafting or building an recipe item on the list will also automatically remove it from the list.

There is also a free text notes field than can be edited in Edit Mode.

The To-Do List is stored in a file `todo-list-for-{PlayerName}-in-{WorldName}-v1.xml` in Valheim's data folder.


## Known issues

Reasons not to use this mod:

 - Missing translations on "Add to To-Do"-button, "Clear Crafting To-Do list"-button and "To-Do" panel title
 - To-Do List does not scale automatically in different display sizes
