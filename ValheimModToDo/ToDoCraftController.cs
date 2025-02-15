using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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

            Jotunn.Logger.LogDebug("Instantiate");
            goAddCraftToDo = Instantiate(craftButton);
            Jotunn.Logger.LogDebug("SetParent");
            goAddCraftToDo.transform.SetParent(craftButton.transform.parent, false);
            Jotunn.Logger.LogDebug("SetName");
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

            Jotunn.Logger.LogDebug("set tooltip");
            goAddCraftToDo.GetComponent<UITooltip>().m_text = "";
        }
    }
}
