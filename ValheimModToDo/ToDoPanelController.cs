using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.Events;

using Logger = Jotunn.Logger;

namespace ValheimModToDo
{
    internal class ToDoPanelController
    {
        public GameObject ToDoEditPanel;
        public GameObject ToDoTextView, ToDoTextEdit;
        public GameObject CleanAllButton;

        readonly float width = 250;
        readonly float height = 600;
        readonly float margin = 10f;
        readonly float headerHeight = 50f;
        readonly float listHeight = 400f;

        public ToDoResources todo;

        private bool IsGuiManagerReady()
        {
            if (GUIManager.Instance == null)
            {
                Logger.LogError("GUIManager instance is null");
                return false;
            }

            if (!GUIManager.CustomGUIFront)
            {
                Logger.LogError("GUIManager CustomGUIFront is null");
                return false;
            }

            if (!GUIManager.CustomGUIBack)
            {
                Logger.LogError("GUIManager CustomGUIBack is null");
                return false;
            }
            return true;
        }

        public void LogWorldName()
        {
            Jotunn.Logger.LogInfo($"WorldName [{ZNet.instance?.GetWorldName()}]");
        }

        public void SetupUi(ToDoResources resources)
        {
            todo = resources;

            Jotunn.Logger.LogInfo($"Make new To-Do panel");
            if (!IsGuiManagerReady())
                return;

            if (ZNet.instance?.GetWorldName() == null)
                return;

            CreateViewModePanel();
            CreateEditModePanel();

            SaveFileLoaded = false;
        }

        public void CreateViewModePanel()
        {
            ToDoTextView = GUIManager.Instance.CreateText(
                text: "Resources",
                parent: GUIManager.CustomGUIBack.transform,
                anchorMin: new Vector2(1f, 0.5f),
                anchorMax: new Vector2(1f, 0.5f),
                position: new Vector2(-width / 2, 0),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: width - 2 * margin,
                height: listHeight,
                addContentSizeFitter: false);
            ToDoTextView.SetActive(false);
        }

        public void CreateEditModePanel()
        {
            // Create the panel object
            ToDoEditPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(1f, 0.5f),
                anchorMax: new Vector2(1f, 0.5f),
                position: new Vector2(-width / 2, 0),
                width: width,
                height: height,
                draggable: true);
            ToDoEditPanel.SetActive(false);
            ToDoEditPanel.AddComponent<DragWindowCntrl>();

            // Header for Edit Mode
            GUIManager.Instance.CreateText(
                text: "To-Do",
                parent: ToDoEditPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -headerHeight),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 100f,
                height: headerHeight,
                addContentSizeFitter: false);

            ToDoTextEdit = GUIManager.Instance.CreateText(
                text: "Resources",
                parent: ToDoEditPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -((listHeight + 2 * headerHeight + 4 * margin) * 0.5f)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: width - 2 * margin,
                height: listHeight,
                addContentSizeFitter: false);

            CleanAllButton = GUIManager.Instance.CreateButton(
                text: "Clear All",
                parent: ToDoEditPanel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0, 40f),
                width: 150f,
                height: 40f);
            CleanAllButton.SetActive(true);

            // Add a listener to the button to close the panel again
            Button button = CleanAllButton.GetComponent<Button>();
            button?.onClick.AddListener(OnClearAllCraftingRecipes);
        }

        private bool SaveFileLoaded = false;

        public void EnsureFileLoaded()
        {
            if (SaveFileLoaded) return;
            SaveFileLoaded = true;
            todo.LoadFromFile();
        }

        public bool Visible = false;

        public void ToggleVisibilty()
        {
            Visible = !Visible;
            UpdateViewModes();
        }

        public bool EditMode = false;

        public void ToggleEditMode()
        {
            EditMode = !EditMode;
            UpdateViewModes();
        }

        public void UpdateViewModes()
        {
            if (Visible)
            {
                UpdateToDoPanel();
                ToDoEditPanel.SetActive(EditMode);
                ToDoTextView.SetActive(!EditMode);
                GUIManager.BlockInput(EditMode);
            }
            else
            {
                ToDoEditPanel.SetActive(false);
                ToDoTextView.SetActive(false);
                GUIManager.BlockInput(false);
            }
        }

        public void UpdateToDoPanel()
        {
            Jotunn.Logger.LogInfo("UpdateToDoPanel");
            var inventory = Player.m_localPlayer?.GetInventory();
            if (inventory != null)
            {
                EnsureFileLoaded();

                UpdateResources(inventory);

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
                if (todo.HasRecipeListChanged())
                    todo.SaveToFile();
            }
            else
            {
                Jotunn.Logger.LogWarning("UpdateToDoPanel: No inventory");
            }
        }

        public void UpdateResources(Inventory inventory)
        {
            var text = GetResourcesText(inventory);

            if (ToDoTextView != null)
            {
                var textComponent = ToDoTextView.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                    Jotunn.Logger.LogInfo("UpdateResourcesText: View mode text updated");
                }
                else
                {
                    Jotunn.Logger.LogError("UpdateResourcesText: No text component");
                }
            }
            else
            {
                Jotunn.Logger.LogWarning("UpdateResourcesText: No text view");
            }

            if (ToDoTextEdit != null)
            {
                var textComponent = ToDoTextEdit.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                    Jotunn.Logger.LogInfo("UpdateResourcesText: Edit mode text updated");
                }
                else
                {
                    Jotunn.Logger.LogError("UpdateResourcesText: No edit mode text component");
                }
            }
            else
            {
                Jotunn.Logger.LogWarning("UpdateResourcesText: No edit mode text view");
            }
        }

        public string GetResourcesText(Inventory inventory)
        {
            Jotunn.Logger.LogInfo("ToDoPanelController.GetResourcesText");
            StringBuilder resourcesText = new("", 2048);

            if (todo.recipes.Count() > 0 || todo.resources.Count() > 0)
            {
                resourcesText.AppendLine(Localization.instance.Localize("$menu_resources:"));
                foreach (var res in todo.resources)
                {
                    if (res.Value > 0)
                    {
                        var id = $"$item_{res.Key.ToLower()}";
                        var name = Localization.instance.Localize(id);
                        var hasInInventory = inventory.CountItems(id);
                        string line;
                        if (hasInInventory < res.Value)
                            line = $"  {name}\t[{hasInInventory} / {res.Value}]";
                        else
                            line = $"  {name}\t[{res.Value}]";
                        Jotunn.Logger.LogInfo(line);
                        resourcesText.AppendLine(line);
                    }
                }

                resourcesText.AppendLine("\n\n");
                resourcesText.AppendLine(Localization.instance.Localize("$inventory_recipes:"));
                foreach (var rec in todo.recipes)
                {
                    if (rec.Value.Count() > 0)
                    {
                        var recipe = rec.Value[0];
                        string is_upgrade = "";
                        if (recipe.quality > 1)
                            is_upgrade = Localization.instance.Localize(" ($piece_upgrade)");
                        resourcesText.AppendLine($"  {recipe.name}{is_upgrade}\t[{rec.Value.Count()}]");
                    }
                }
            }

            var text = resourcesText.ToString();

            Jotunn.Logger.LogInfo($"To-Do List:\n{text}");
            return text;
        }
        private void OnClearAllCraftingRecipes()
        {
            todo.ClearRecipes();
            UpdateToDoPanel();
        }

        public bool InventoryGuiOpen = false;

        public void OnShowInventoryGui()
        {
            if (ToDoTextView != null)
            {
                if (InventoryGuiOpen == false)
                {
                    InventoryGuiOpen = true;
                    // ToDoTextView.transform.Translate(-width, 0f, 0f);
                    var rect = ToDoTextView.GetComponent<RectTransform>();
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(width / 2, 0);
                }
            }
        }

        public void OnHideInventoryGui()
        {
            if (ToDoTextView != null)
            {
                if (InventoryGuiOpen == true)
                {
                    InventoryGuiOpen = false;
                    // ToDoTextView.transform.Translate(width, 0f, 0f);
                    var rect = ToDoTextView.GetComponent<RectTransform>();
                    rect.anchorMax = new Vector2(1f, 0.5f);
                    rect.anchorMin = new Vector2(1f, 0.5f);
                    rect.anchoredPosition = new Vector2(-width / 2, 0);
                }
            }
        }

    }
}
