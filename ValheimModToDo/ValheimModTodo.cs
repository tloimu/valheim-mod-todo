using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Jotunn.Configs;
using System;
using HarmonyLib;
using System.Reflection;

using Logger = Jotunn.Logger;
using UnityEngine.InputSystem;

namespace ValheimModToDo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ValheimModToDo : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.ValheimModToDo";
        public const string PluginName = "ValheimModToDo";
        public const string PluginVersion = "0.6.0";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private static ValheimModToDo _instance;

        private readonly ToDoResources todoResources = new();

        private readonly ToDoPanelController todoPanel = new();

        private void Awake()
        {
            _instance = this;
            Jotunn.Logger.LogInfo("To-Do List Mod Awake");
            AddInputs();
            GUIManager.OnCustomGUIAvailable += OnRebuildUi;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "tloimu.mods.todo");
        }

        public void OnRebuildUi()
        {
            Jotunn.Logger.LogInfo($"OnRebuildUi [{this.GetInstanceID()}]");
            todoPanel.SetupUi(todoResources);
        }

        private void OnDestroy()
        {
            Jotunn.Logger.LogInfo("To-Do List Mod OnDestroy");
            GUIManager.OnCustomGUIAvailable -= OnRebuildUi;
            _instance = null;
        }

        private ButtonConfig ToggleVisibiltyButton;
        private ButtonConfig AddCraftToDoButton;

        private void AddInputs()
        {
            // Add key bindings on the fly
            ToggleVisibiltyButton = new ButtonConfig
            {
                Name = "Open To-Do Panel",
                Key = KeyCode.Home,
                ActiveInCustomGUI = true
            };
            InputManager.Instance.AddButton(PluginGUID, ToggleVisibiltyButton);

            AddCraftToDoButton = new ButtonConfig
            {
                Name = "Add Crafting to To-Do list",
                Key = KeyCode.Insert,
                ActiveInCustomGUI = true
            };
            InputManager.Instance.AddButton(PluginGUID, AddCraftToDoButton);
        }

        private void Update()
        {
            if (ZInput.instance != null)
            {
                if (ZInput.GetButtonDown(ToggleVisibiltyButton.Name))
                {
                    todoPanel.ToggleVisibilty();
                }
                if (ZInput.GetButtonDown(AddCraftToDoButton.Name))
                {
                    AddCurrentSelectionToDoList();
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
                if (piece.name == "piece_repair") // Repair function is a build piece in Valheim
                    return;
                Jotunn.Logger.LogInfo($"AddCurrentCraftItemToDoList: {piece.name}, total {total}");
                todoResources.AddRecipe(piece);
                UpdateToDoPanel();
            }
        }

        public void UpdateToDoPanel()
        {
            todoPanel.UpdateToDoPanel();
        }

        public static void OnClickAddCraftItemButton()
        {
            Jotunn.Logger.LogInfo("OnClickAddCraftItemButton");
            var gui = InventoryGui.instance;
            var selectedRecipe = gui.m_selectedRecipe;
            var selectedVariant = gui.m_selectedVariant;
            int qualityLevel = 1;
            bool multiCrafting = false;
            if (gui.InUpradeTab())
            {
                var upgradeItem = selectedRecipe.ItemData;
                qualityLevel = ((upgradeItem == null) ? 1 : (upgradeItem.m_quality + 1));
            }
            else
            {
                multiCrafting = gui.m_craftUpgradeItem == null && (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyLStick"));
            }
            Jotunn.Logger.LogInfo($"m_selectedRecipe={selectedRecipe.Recipe}");
            Jotunn.Logger.LogInfo($"m_selectedVariant={selectedVariant}");
            Jotunn.Logger.LogInfo($"m_craftRecipe.m_craftingStation={selectedRecipe.Recipe?.m_craftingStation}");
            Jotunn.Logger.LogInfo($"qualityLevel={qualityLevel}");
            int itemCount = multiCrafting ? gui.m_multiCraftAmount : 1;
            if (_instance != null)
                _instance.AddCraftToDo(selectedRecipe.Recipe, qualityLevel, itemCount);
        }

        public static void OnRemoveCraftToDo(Recipe recipe, int quality = 1, int count = 1)
        {
            if (_instance != null)
                _instance.RemoveCraftToDo(recipe, quality, count);
        }

        public static void OnRemoveCraftToDo(Piece piece, int quality = 1)
        {
            if (_instance != null)
                _instance.RemoveCraftToDo(piece, quality);
        }

        public static void OnInventoryChanged()
        {
            if (_instance != null)
                _instance.UpdateToDoPanel();
        }

        public static void OnShowInventory()
        {
            if (_instance != null && _instance.todoPanel != null)
                _instance.todoPanel.OnShowInventoryGui();
        }
        public static void OnHideInventory()
        {
            if (_instance != null && _instance.todoPanel != null)
                _instance.todoPanel.OnHideInventoryGui();
        }

        private void AddCraftToDo(Recipe recipe, int quality, int count = 1)
        {
            Jotunn.Logger.LogInfo($"Add Craft To-Do: {recipe.name} amount {recipe.m_amount} quality {quality}");
            for (int i = 0; i < count; i++)
                todoResources.AddRecipe(recipe, quality);
            UpdateToDoPanel();
        }

        private void RemoveCraftToDo(Recipe recipe, int quality, int count = 1)
        {
            Jotunn.Logger.LogInfo($"Remove Craft To-Do: {recipe.name} quality {quality}");
            for (int i = 0; i < count; i++)
                todoResources.RemoveRecipe(recipe, quality);
            UpdateToDoPanel();
        }

        private void RemoveCraftToDo(Piece piece, int quality)
        {
            Jotunn.Logger.LogInfo($"Remove Build To-Do: {piece.name} quality {quality}");
            todoResources.RemoveRecipe(piece.name, quality);
            UpdateToDoPanel();
        }
   }
}