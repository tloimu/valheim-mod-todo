using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

namespace ValheimModToDo.Patches
{
    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    public static class InventoryGui_Awake_Patch
    {
        public static void Postfix(InventoryGui __instance)
        {
            ToDoCraftController.CreateAddCraftToDoButton(ValheimModToDo.OnClickAddCraftItemButton);
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
    public static class Inventory_Changed_Patch
    {
        public static void Postfix(Inventory __instance)
        {
            Jotunn.Logger.LogInfo($"Inventory.Changed: name {__instance.m_name}");
            if (Player.m_localPlayer != null)
            {
                if (Player.m_localPlayer.m_inventory == __instance)
                    ValheimModToDo.OnInventoryChanged();
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    public static class InventoryGui_DoCrafting_Patch
    {
        public static void Postfix(InventoryGui __instance, Player player)
        {
            Jotunn.Logger.LogInfo("InventoryGui.DoCrafting()");
            var recipe = __instance.m_selectedRecipe.Recipe;
            var craftUpgradeItem = __instance.m_craftUpgradeItem;
            var qualityLevel = (craftUpgradeItem == null) ? 1 : (craftUpgradeItem.m_quality + 1);
            if (recipe)
            {
                Jotunn.Logger.LogInfo($"InventoryGui.DoCrafting {recipe.name} quality {qualityLevel}");
                foreach (var res in recipe.m_resources)
                {
                    Jotunn.Logger.LogInfo($"  - {res.m_resItem.name} [{res.m_amount}]");
                }
                ValheimModToDo.OnRemoveCraftToDo(recipe.name);
            }
        }
    }

/*    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.DropItem))]
    public static class Humanoid_DropItem_Patch
    {
        public static void Postfix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, int amount)
        {
            Jotunn.Logger.LogInfo($"Humanoid({__instance.name}).DropItem: {item.m_shared.m_name}");
            if (__instance.IsPlayer())
                ValheimModToDo.OnInventoryChanged();
        }
    }*/

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    public static class Player_PlacePiece_Patch
    {
        public static void Postfix(Player __instance, Piece piece, Vector3 pos, Quaternion rot, bool doAttack)
        {
            Jotunn.Logger.LogInfo($"Player.PlacePiece: {piece.name}");
            foreach (var res in piece.m_resources)
            {
                Jotunn.Logger.LogInfo($"  - {res.m_resItem.name} [{res.m_amount}]");
            }
            ValheimModToDo.OnRemoveCraftToDo(piece.name);
        }
    }
}
