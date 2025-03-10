﻿using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace ValheimModToDo
{
    internal class ToDoCraftController
    {
        public static GameObject goAddCraftToDo;
        public static Button btnAddCraftToDo;
        public static TMP_Text txtAddCraftToDo;
        public static RectTransform InventoryPanelPos;

        public static void CreateAddCraftToDoButton(UnityAction onClickAddCraftItemButton)
        {
            var craftButton = InventoryGui.instance?.m_craftButton?.gameObject;
            if (craftButton == null)
            {
                Jotunn.Logger.LogError("craftButton not found");
                return;
            }

            if (goAddCraftToDo != null)
            {
                Jotunn.Logger.LogDebug("craftAddCraftToDoButton already exists. destroy it.");
                Destroy(goAddCraftToDo);
            }

            InventoryPanelPos = InventoryGui.instance?.gameObject?.GetComponent<RectTransform>();

            goAddCraftToDo = Instantiate(craftButton);
            goAddCraftToDo.transform.SetParent(craftButton.transform.parent, false);
            goAddCraftToDo.name = "craftAddToDoButton";

            var position = goAddCraftToDo.transform.position;
            position.x += 0f;
            position.y += -60f;
            goAddCraftToDo.transform.position = position;

            var rect = goAddCraftToDo.GetComponent<RectTransform>();
            var size = rect.sizeDelta;
            size.x += -150;
            size.y += -20;
            rect.sizeDelta = size;

            btnAddCraftToDo = goAddCraftToDo.GetComponentInChildren<Button>();
            btnAddCraftToDo.interactable = true;
            btnAddCraftToDo.onClick.AddListener(onClickAddCraftItemButton);

            txtAddCraftToDo = goAddCraftToDo.GetComponentInChildren<TMP_Text>();
            if (txtAddCraftToDo != null)
            {
                txtAddCraftToDo.text = "Add to To-Do";
                txtAddCraftToDo.autoSizeTextContainer = false;
                txtAddCraftToDo.fontSize = 16;
            }

            goAddCraftToDo.GetComponent<UITooltip>().m_text = "";
        }
    }
}
