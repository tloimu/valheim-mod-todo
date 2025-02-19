﻿using Jotunn.GUI;
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
        public GameObject ToDoPanel;
        public GameObject ToDoTextView;
        public GameObject CleanAllButton;

        readonly float width = 250;
        readonly float height = 600;
        readonly float margin = 10f;
        readonly float headerHeight = 50f;

        public GameObject CreatePanel(UnityAction onClearAllCraftingRecipes)
        {
            Jotunn.Logger.LogInfo("Make new To-Do panel");
            if (GUIManager.Instance == null)
            {
                Logger.LogError("GUIManager instance is null");
                return null;
            }

            if (!GUIManager.CustomGUIFront)
            {
                Logger.LogError("GUIManager CustomGUI is null");
                return null;
            }

            // Create the panel object
            var panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(1f, 0.5f),
                anchorMax: new Vector2(1f, 0.5f),
                position: new Vector2(-width, 0),
                width: width,
                height: height,
                draggable: true);
            panel.SetActive(false);

            // Add the Jötunn draggable Component to the panel
            // Note: This is normally automatically added when using CreateWoodpanel()
            DragWindowCntrl.ApplyDragWindowCntrl(panel);

            // Create the text object
            GUIManager.Instance.CreateText(
                text: "To-Do",
                parent: panel.transform,
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

            // Create the text object
            float listHeight = 400f;
            ToDoTextView = GUIManager.Instance.CreateText(
                text: "Resources",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -((listHeight + 2*headerHeight + 4*margin)*0.5f)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: width - 2 * margin,
                height: listHeight,
                addContentSizeFitter: false);

            // Create the button object
            CleanAllButton = GUIManager.Instance.CreateButton(
                text: "Clear All",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0, 40f),
                width: 150f,
                height: 40f);
            CleanAllButton.SetActive(false);

            // Add a listener to the button to close the panel again
            Button button = CleanAllButton.GetComponent<Button>();
            button.onClick.AddListener(onClearAllCraftingRecipes);

            ToDoPanel = panel;
            return panel;
        }

        public void SetActive(bool active)
        {
            ToDoPanel?.SetActive(active);
            if (!active)
                SetEditMode(false);
        }

        public bool EditMode = false;

        public void SetEditMode(bool active)
        {
            EditMode = active;
            CleanAllButton?.SetActive(EditMode);
            GUIManager.BlockInput(active);
        }

        public void UpdateResources(ToDoResources todo, Inventory inventory)
        {
            if (ToDoTextView != null)
            {
                var textComponent = ToDoTextView.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = GetResourcesText(todo, inventory);
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
        }

        public string GetResourcesText(ToDoResources todo, Inventory inventory)
        {
            Jotunn.Logger.LogInfo("ToDoPanelController.GetResourcesText");
            StringBuilder resourcesText = new("", 2048);

            if (todo.recipes.Count() > 0 || todo.resources.Count() > 0)
            {
                resourcesText.AppendLine(Localization.instance.Localize("$menu_resources:"));
                foreach (var res in todo.resources)
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

                resourcesText.AppendLine("\n\n");
                resourcesText.AppendLine(Localization.instance.Localize("$inventory_recipes:"));
                foreach (var rec in todo.recipes)
                {
                    if (rec.Value.Count() > 0)
                        resourcesText.AppendLine($"  {rec.Value[0].name}\t[{rec.Value.Count()}]");
                }
            }

            var text = resourcesText.ToString();

            Jotunn.Logger.LogInfo($"To-Do List:\n{text}");
            return text;
        }
    }
}
