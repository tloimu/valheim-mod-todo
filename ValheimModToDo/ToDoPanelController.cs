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
        public GameObject ToDoViewPanel;
        public GameObject ToDoTextNotes;
        public GameObject ClearAllButton;
        public ToDoListEdit ListEditor = new();
        public ToDoListEdit ListViewer = new();

        readonly float width = 282f;
        readonly float buttonWidth = 16f;
        readonly float nameWidth = 190f;
        readonly float height = 600;
        readonly float margin = 10f;
        readonly float notesHeight = 60f;
        readonly float headerHeight = 50f;
        readonly float listHeight = 300f;

        public ToDoResources todo = new();

        public bool gLogVerbose = false;

        public ToDoPanelController()
        {
            Logger.LogInfo($"Constructor ToDoPanelController");
        }

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

        public void SetupUi()
        {
            Jotunn.Logger.LogInfo($"Make new To-Do panel");
            if (!IsGuiManagerReady())
                return;

            if (ZNet.instance?.GetWorldName() == null)
            {
                Jotunn.Logger.LogDebug($"World not ready yet");
            }

            CreateViewModePanel();
            CreateEditModePanel();

            SaveFileLoaded = false;
        }

        public void CreateViewModePanel()
        {
            if (ToDoViewPanel == null)
            {
                ToDoViewPanel = DefaultControls.CreatePanel(GUIManager.Instance.ValheimControlResources);
                ToDoViewPanel.transform.SetParent(GUIManager.CustomGUIBack.transform, false);
                ToDoViewPanel.GetComponent<Image>().pixelsPerUnitMultiplier = 1f;
                ToDoViewPanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.0f);

                var tf = (RectTransform)ToDoViewPanel.transform;
                tf.anchoredPosition = new Vector2(-width / 2, 0);
                tf.anchorMin = new Vector2(1f, 0.5f);
                tf.anchorMax = new Vector2(1f, 0.5f);
                tf.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                tf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

                ToDoViewPanel.SetActive(false);

                ListViewer.todoList = todo;
                ListViewer.AddViewMode(ToDoViewPanel, width, height, nameWidth);

                Jotunn.Logger.LogDebug("ToDoPanelController: View Panel Created");
            }
        }

        public void CreateEditModePanel()
        {
            if (ToDoEditPanel == null)
            {
                // Create the panel object
                ToDoEditPanel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(width / 2, 0),
                    width: width + 2 * buttonWidth,
                    height: height,
                    draggable: false);
                ToDoEditPanel.SetActive(false);

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

                ToDoTextNotes = GUIManager.Instance.CreateInputField(
                    parent: ToDoEditPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -(notesHeight + 2 * headerHeight + 4 * margin) * 0.5f),
                    width: width - 2 * margin,
                    height: notesHeight
                    );
                var inputComponent = ToDoTextNotes.GetComponent<InputField>();
                inputComponent.lineType = InputField.LineType.MultiLineNewline;
                inputComponent.shouldActivateOnSelect = true;
                inputComponent.onEndEdit.AddListener((string text) => GUIManager.BlockInput(false));

                ListEditor.todoList = todo;
                ListEditor.AddEditMode(ToDoEditPanel, width - 2 * margin, listHeight, nameWidth);
                ListEditor.onListChanged.AddListener(UpdateToDoPanel);

                ClearAllButton = GUIManager.Instance.CreateButton(
                    text: "Clear All",
                    parent: ToDoEditPanel.transform,
                    anchorMin: new Vector2(0.5f, 0f),
                    anchorMax: new Vector2(0.5f, 0f),
                    position: new Vector2(0, 40f),
                    width: 150f,
                    height: 40f);
                ClearAllButton.SetActive(true);

                Button button = ClearAllButton.GetComponent<Button>();
                button?.onClick.AddListener(OnClearAllCraftingRecipes);

                Jotunn.Logger.LogDebug("ToDoPanelController: Edit Panel Created");
            }
        }

        private bool isEditingNotes = false;

        public void CheckTextInput()
        {
            if (ToDoTextNotes == null) return;

            var inputComponent = ToDoTextNotes.GetComponent<InputField>();
            if (isEditingNotes)
            {
                if (!inputComponent.isFocused)
                {
                    isEditingNotes = false;
                    GUIManager.BlockInput(false);
                }
            }
            else
            {
                if (inputComponent.isFocused)
                {
                    isEditingNotes = true;
                    GUIManager.BlockInput(true);
                }
            }
        }

        private bool SaveFileLoaded = false;

        public void EnsureFileLoaded()
        {
            if (SaveFileLoaded) return;
            SaveFileLoaded = true;
            todo.LoadFromFile();
            SetToDoNotesToUi();
        }

        public bool Visible = false;

        public void ToggleVisibilty()
        {
            Visible = !Visible;
            UpdateViewModes();
        }

        public void SetVisible(bool visible = true)
        {
            if (Visible != visible)
            {
                Visible = visible;
                UpdateViewModes();
            }
        }

        public void UpdateViewModes()
        {
            if (ToDoEditPanel == null || ToDoViewPanel == null)
            {
                SetupUi();
                return;
            }

            if (Visible)
            {
                UpdateToDoPanel();
                ToDoEditPanel.SetActive(InventoryGuiOpen);
                ToDoViewPanel.SetActive(!InventoryGuiOpen);
            }
            else
            {
                ToDoEditPanel.SetActive(false);
                ToDoViewPanel.SetActive(false);
            }
        }

        public void UpdateToDoPanel()
        {
            Jotunn.Logger.LogInfo("UpdateToDoPanel");
            var inventory = Player.m_localPlayer?.GetInventory();
            if (inventory != null)
            {
                if (ToDoViewPanel == null || ToDoEditPanel == null)
                    SetupUi();

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

                if (todo.WasChangedSince())
                    todo.SaveToFile();
            }
            else
            {
                Jotunn.Logger.LogWarning("UpdateToDoPanel: No inventory");
            }
        }

        public void UpdateResources(Inventory inventory)
        {
            Jotunn.Logger.LogInfo($"UpdateResources");
            if (ListEditor != null)
            {
                UpdateListView(inventory, ListEditor);
            }
            else
            {
                Jotunn.Logger.LogInfo("UpdateResources: ListEditor NULL");
            }

            if (ListViewer != null)
            {
                UpdateListView(inventory, ListViewer);
            }
            else
            {
                Jotunn.Logger.LogInfo("UpdateResources: ListViewer NULL");
            }
        }

        public string GetResourcesText(Inventory inventory, bool includeNotes = true)
        {
            Jotunn.Logger.LogInfo("ToDoPanelController.GetResourcesText");
            StringBuilder resourcesText = new("", 2048);

            if (includeNotes && todo.notes != null && todo.notes.Length > 0)
            {
                resourcesText.AppendLine(todo.notes);
                resourcesText.AppendLine();
            }

            if (todo.recipes.Count() > 0 || todo.resources.Count() > 0)
            {
                resourcesText.AppendLine(Localization.instance.Localize("$menu_resources:"));
                foreach (var res in todo.resources)
                {
                    if (res.Value.count > 0)
                    {
                        var name = res.Value.item.name;
                        var hasInInventory = inventory.CountItems(res.Value.item.id);
                        string line;
                        if (hasInInventory < res.Value.count)
                            line = $"  {name}\t[{hasInInventory} / {res.Value.count}]";
                        else
                            line = $"  {name}\t[{res.Value.count}]";
                        if (gLogVerbose)
                                Jotunn.Logger.LogInfo($"{line} from key [{res.Key}] id [{res.Value.item.id}] name [{res.Value.item.name}]");
                        resourcesText.AppendLine(line);
                    }
                }

                resourcesText.AppendLine();
                resourcesText.AppendLine(Localization.instance.Localize("$inventory_recipes:"));
                foreach (var rec in todo.recipes)
                {
                    if (rec.Value.Count() > 0)
                    {
                        var recipe = rec.Value[0];
                        string is_upgrade = "";
                        if (recipe.quality > 1)
                            is_upgrade = $" ⇧{recipe.quality}";
                        resourcesText.AppendLine($"  {recipe.name}{is_upgrade}\t[{rec.Value.Count()}]");
                    }
                }
            }

            var text = resourcesText.ToString();

            if (gLogVerbose)
            {
                Jotunn.Logger.LogInfo($"To-Do List:\n{text}");
                Jotunn.Logger.LogInfo($"Inventory:");
                foreach (var item in inventory.m_inventory)
                {
                    Jotunn.Logger.LogInfo($"  - [{item.m_shared.m_name}]");
                }
            }
            return text;
        }

        public void UpdateListView(Inventory inventory, ToDoListEdit listView)
        {
            Jotunn.Logger.LogInfo("ToDoPanelController.UpdateListView");
            listView.Clear();

            if (listView.viewOnly)
            {
                listView.AddLabelRow(todo.notes);
                listView.AddLabelRow("");
            }


            if (todo.resources.Count > 0)
            {
                listView.AddLabelRow(Localization.instance.Localize("$menu_resources:"));

                foreach (var res in todo.resources)
                {
                    if (res.Value.count > 0)
                    {
                        var name = res.Value.item.name;
                        var hasInInventory = inventory.CountItems(res.Value.item.id);
                        string line;
                        if (hasInInventory < res.Value.count)
                            line = $"[{hasInInventory} / {res.Value.count}]";
                        else
                            line = $"[{res.Value.count}]";
                        listView.AddRow($"  {name}", line);
                    }
                }
            }

            if (todo.recipes.Count > 0)
            {
                listView.AddLabelRow("");
                listView.AddLabelRow(Localization.instance.Localize("$inventory_recipes:"));

                foreach (var rec in todo.recipes)
                {
                    if (rec.Value.Count() > 0)
                    {
                        var recipe = rec.Value[0];
                        string is_upgrade = "";
                        if (recipe.quality > 1)
                            is_upgrade = $" ⇧{recipe.quality}";
                        if (listView.viewOnly)
                            listView.AddRow($"  {recipe.name}{is_upgrade}", $"[{rec.Value.Count()}]");
                        else
                            listView.AddRow($"  {recipe.name}{is_upgrade}", $"[{rec.Value.Count()}]", recipe.id);
                    }
                }
            }
        }

        private void OnClearAllCraftingRecipes()
        {
            todo.ClearRecipes();
            UpdateToDoPanel();
        }

        public bool InventoryGuiOpen = false;

        public void OnShowInventoryGui()
        {
            if (InventoryGuiOpen == false)
            {
                InventoryGuiOpen = true;
                UpdateViewModes();
            }
        }

        public void OnHideInventoryGui()
        {
            if (InventoryGuiOpen == true)
            {
                InventoryGuiOpen = false;
                GetToDoNotesFromUi();
                UpdateViewModes();
            }
        }

        public void GetToDoNotesFromUi()
        {
            if (ToDoTextNotes != null)
            {
                var input = ToDoTextNotes.GetComponent<InputField>();
                if (input != null && input.textComponent is UnityEngine.UI.Text text)
                {
                    todo.SetNotes(input.text);
                }
                else
                {
                    Jotunn.Logger.LogError("GetToDoNotesFromUi: No ToDoTextNotes text component");
                }
            }
        }

        public void SetToDoNotesToUi()
        {
            if (ToDoTextNotes != null)
            {
                var input = ToDoTextNotes.GetComponent<InputField>();
                if (input != null && input.textComponent is UnityEngine.UI.Text text)
                {
                    input.text = todo.notes;
                }
                else
                {
                    Jotunn.Logger.LogError("SetToDoNotesToUi: No ToDoTextNotes text component");
                }
            }
        }
    }
}
