using HarmonyLib;
using UnityEngine;

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
            if (Player.m_localPlayer != null && !Player.m_localPlayer.m_isLoading)
            {
                Jotunn.Logger.LogInfo($"Inventory.Changed: name {__instance.m_name}");
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
            if (player != Player.m_localPlayer)
                return;
            var recipe = __instance.m_selectedRecipe.Recipe;
            var craftUpgradeItem = __instance.m_craftUpgradeItem;
            var qualityLevel = (craftUpgradeItem == null) ? 1 : (craftUpgradeItem.m_quality + 1);
            if (recipe)
            {
                Jotunn.Logger.LogInfo($"InventoryGui.DoCrafting({recipe.name})");
                ValheimModToDo.OnRemoveCraftToDo(recipe, qualityLevel);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
    public static class InventoryGui_Show_Patch
    {
        public static void Postfix(InventoryGui __instance, Container container, int activeGroup)
        {
            ValheimModToDo.OnShowInventory();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    public static class InventoryGui_Hide_Patch
    {
        public static void Postfix(InventoryGui __instance)
        {
            ValheimModToDo.OnHideInventory();
        }
    }


    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    public static class Player_PlacePiece_Patch
    {
        public static void Postfix(Player __instance, Piece piece, Vector3 pos, Quaternion rot, bool doAttack)
        {
            if (__instance != Player.m_localPlayer)
                return;
            Jotunn.Logger.LogInfo($"Player.PlacePiece: {piece.name}");
            ValheimModToDo.OnRemoveCraftToDo(piece);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnRespawn))]
    public static class Player_OnRespawn_Patch
    {
        public static void Postfix(Player __instance)
        {
            Jotunn.Logger.LogInfo($"Player.OnRespawn [{__instance.name}]");
            if (__instance != Player.m_localPlayer)
                return;
            ValheimModToDo.OnInventoryChanged();
        }
    }
}
