using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Jotunn.Configs;
using System;
using HarmonyLib;
using System.Reflection;

using Logger = Jotunn.Logger;

namespace ValheimModToDo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ValheimModToDo : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.ValheimModToDo";
        public const string PluginName = "ValheimModToDo";
        public const string PluginVersion = "0.3.0";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private static ValheimModToDo _instance;

        private ToDoResources todoResources = new();

        private ToDoPanelController todoPanel = new();

        private void Awake()
        {
            _instance = this;
            Jotunn.Logger.LogInfo("To-Do List Mod Awake");
            AddInputs();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "tloimu.mods.todo");
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private ButtonConfig ShowGUIButton;
        private ButtonConfig AddCraftToDoButton;
        private ButtonConfig ClearAllCraftToDoButton;

        private void AddInputs()
        {
            // Add key bindings on the fly
            ShowGUIButton = new ButtonConfig
            {
                Name = "Open To-Do Panel",
                Key = KeyCode.Home,
                ActiveInCustomGUI = true
            };
            InputManager.Instance.AddButton(PluginGUID, ShowGUIButton);

            AddCraftToDoButton = new ButtonConfig
            {
                Name = "Add Crafting to To-Do list",
                Key = KeyCode.Insert,
                ActiveInCustomGUI = true
            };
            InputManager.Instance.AddButton(PluginGUID, AddCraftToDoButton);

            ClearAllCraftToDoButton = new ButtonConfig
            {
                Name = "Clear Crafting To-Do list",
                Key = KeyCode.Delete,
                ActiveInCustomGUI = true
            };
            InputManager.Instance.AddButton(PluginGUID, ClearAllCraftToDoButton);
        }

        private void Update()
        {
            if (ZInput.instance != null)
            {
                if (ZInput.GetButtonDown(ShowGUIButton.Name))
                {
                    TogglePanel();
                }
                if (ZInput.GetButtonDown(AddCraftToDoButton.Name))
                {
                    AddCurrentSelectionToDoList();
                }
                if (ZInput.GetButtonDown(ClearAllCraftToDoButton.Name))
                {
                    OnClearAllCraftingRecipes();
                }
            }
        }

        private void AddCurrentSelectionToDoList()
        {
            Jotunn.Logger.LogInfo("Add Current Craft Item to To-Do list");
            // ???? TODO: Determine if Build panel is open and if the current item can be
            // ???? TODO: determined from there and add it to the list if possible.
            Player.m_localPlayer.GetBuildSelection(out var piece, out var id, out var total, out var category, out var pieceTable);
            if (piece != null)
            {
                Jotunn.Logger.LogInfo($"AddCurrentCraftItemToDoList: {piece.name}, total {total}");
                todoResources.AddRecipe(piece);
                UpdateToDoPanel();
            }
        }

        private GameObject ToDoPanel;

        private void TogglePanel()
        {
            Jotunn.Logger.LogInfo("Toggle To-Do panel");
            // Create the panel if it does not exist
            if (!ToDoPanel)
            {
                todoResources.LoadFromFile();
                ToDoPanel = todoPanel.CreatePanel(OnClearAllCraftingRecipes);
            }

            // Switch the current state
            bool state = !ToDoPanel.activeSelf;

            if (state)
                UpdateToDoPanel();

            // Set the active state of the panel
            todoPanel.SetActive(state);

            // Toggle input for the player and camera while displaying the GUI
            // GUIManager.BlockInput(state);
        }

        public void UpdateToDoPanel()
        {
            var inventory = Player.m_localPlayer.GetInventory();
            todoPanel.UpdateResources(todoResources, inventory);

            if (InventoryGui.instance != null && InventoryGui.instance.IsContainerOpen())
            {
                var container = InventoryGui.instance.m_currentContainer;
                if (container != null)
                {
                    var containerInventory = container.GetInventory();
                    if (containerInventory != null)
                    {
                        Jotunn.Logger.LogInfo($"Container: {container.name} [{container.m_name}] id [{container.GetInstanceID()}]");
                        foreach (var item in containerInventory.m_inventory)
                        {
                            Jotunn.Logger.LogInfo($"  {item.m_shared.m_name} stack {item.m_stack} quality {item.m_quality}");
                        }
                    }
                }
            }
            if (todoResources.HasRecipeListChanged())
                todoResources.SaveToFile();
        }

        public static void OnClickAddCraftItemButton()
        {
            Jotunn.Logger.LogInfo("OnClickAddCraftItemButton");

            var gui = InventoryGui.instance;
            var selectedRecipe = gui.m_selectedRecipe;
            var selectedVariant = gui.m_selectedVariant;
            Jotunn.Logger.LogInfo($"m_selectedRecipe={selectedRecipe.Recipe}");
            Jotunn.Logger.LogInfo($"m_selectedVariant={selectedVariant}");
            Jotunn.Logger.LogInfo($"m_craftRecipe.m_craftingStation={selectedRecipe.Recipe?.m_craftingStation}");
            var recipe = selectedRecipe.Recipe;
            if (recipe.m_item != null && recipe.m_item.m_piece != null)
            {
                Jotunn.Logger.LogInfo($"m_piece={recipe.m_item.m_piece.m_name}");
                var resources = recipe.m_item.m_piece.m_resources;
                foreach (var res in resources)
                {
                    // recipe.GetAmount(qualityLevel, out var need);
                    Jotunn.Logger.LogInfo($"  - {res.m_resItem.name} [{res.m_amount}]");
                }
            }
            if (_instance != null)
                _instance.AddCraftToDo(recipe);
        }

        public static void OnRemoveCraftToDo(String name)
        {
            if (_instance != null)
                _instance.RemoveCraftToDo(name);
        }

        public static void OnInventoryChanged()
        {
            if (_instance != null)
                _instance.UpdateToDoPanel();
        }

        private void OnClearAllCraftingRecipes()
        {
            todoResources.ClearRecipes();
            UpdateToDoPanel();
        }

        private void AddCraftToDo(Recipe recipe)
        {
            Jotunn.Logger.LogInfo($"Add Craft To-Do: {recipe.name} amount {recipe.m_amount} item {recipe.m_item.name}");
            var resources = recipe.m_resources;
            foreach (var res in resources)
            {
                Jotunn.Logger.LogInfo($"  - {res.m_resItem.name} [{res.m_amount}]");
            }
            todoResources.AddRecipe(recipe);
            UpdateToDoPanel();
        }

        private void RemoveCraftToDo(String name)
        {
            Jotunn.Logger.LogInfo($"Remove Craft To-Do: {name}");
            todoResources.RemoveRecipe(name);
            UpdateToDoPanel();
        }
    }
}