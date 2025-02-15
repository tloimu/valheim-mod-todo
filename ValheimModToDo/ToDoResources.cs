using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using System.Reflection;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using static ClutterSystem;
using Jotunn.Configs;
using Mono.Cecil;

namespace ValheimModToDo
{
    internal class ToDoResource
    {
        public string id = "";
        public string name = "";
        public int amount = 1;
        public ToDoResource(string id, string name, int amount)
        {
            this.id = id;
            this.name = name ?? id;
            this.amount = amount;
        }

        public ToDoResource(Piece.Requirement req, int quality)
        {
            id = req.m_resItem.name;
            name = id; // ???? TODO: Get translation?
            amount = req.GetAmount(quality);

        }
    }

    internal class ToDoRecipe
    {
        public string id = "";
        public string name = "";
        public int quality = 1;
        public List<ToDoResource> resources = new();

        public ToDoRecipe(string id, string name = null, int quality = 1)
        {
            this.id = id;
            this.name = name ?? id;
            this.quality = quality;
        }

        public ToDoRecipe(Piece piece)
        {
            id = piece.name;
            name = piece.name; // ???? TODO: Get translation?
            quality = 1;
            foreach (var resource in piece.m_resources)
            {
                resources.Add(new ToDoResource(resource, quality));
            }
        }

        public ToDoRecipe(Recipe recipe)
        {
            id = ToDoRecipe.GetRecipeId(recipe);
            name = recipe.name; // ???? TODO: Get translation?
            if (recipe.m_item != null && recipe.m_item.m_itemData != null)
                quality = recipe.m_item.m_itemData.m_quality;
            else
                quality = 1;

            if (recipe.m_item != null && recipe.m_item.m_piece != null)
            {
                foreach (var resource in recipe.m_item.m_piece.m_resources)
                {
                    resources.Add(new ToDoResource(resource, quality));
                }
            }
            else
            {
                foreach (var resource in recipe.m_resources)
                {
                    resources.Add(new ToDoResource(resource, quality));
                }
            }
        }

        static public string GetRecipeId(Recipe recipe)
        {
            return recipe.name; // ???? TODO: Maybe add quality to this?
        }
    }


    internal class ToDoResources
    {
        public Dictionary<string, List<ToDoRecipe>> recipes = new();
        public Dictionary<string, int> resources = new();

        private readonly object _recipeLock = new();

        public void ClearRecipes()
        {
            lock (_recipeLock)
            {
                recipes.Clear();
                resources.Clear();
            }
        }

        public void AddRecipe(Recipe recipe)
        {
            AddRecipe(new ToDoRecipe(recipe));
        }

        public void AddRecipe(Piece piece)
        {
            AddRecipe(new ToDoRecipe(piece));
        }

        public void AddRecipe(ToDoRecipe recipe)
        {
            Jotunn.Logger.LogInfo($"ToDoResources: AddRecipe({recipe.name})");
            lock (_recipeLock)
            {
                if (recipes.TryGetValue(recipe.id, out var list))
                {
                    list.Add(recipe);
                    Jotunn.Logger.LogInfo($"ToDoResources: Added Recipe {recipe.name}");
                }
                else
                {
                    recipes.Add(recipe.id, new List<ToDoRecipe> { recipe });
                }
                foreach (var resource in recipe.resources)
                {
                    if (resource.amount > 0)
                    {
                        if (resources.TryGetValue(resource.id, out var amount))
                            resources[resource.id] = amount + resource.amount;
                        else
                            resources[resource.id] = resource.amount;
                        Jotunn.Logger.LogInfo($"ToDoResources: Added Resource {resource.id} amount {amount} now need {resources[resource.id]}");
                    }
                }
            }
        }

        public void RemoveRecipe(Recipe recipe)
        {
            RemoveRecipe(ToDoRecipe.GetRecipeId(recipe));
        }

        public void RemoveRecipe(string id)
        {
            Jotunn.Logger.LogInfo($"ToDoResources: RemoveRecipe({id})");
            lock (_recipeLock)
            {
                if (recipes.TryGetValue(id, out var foundRecipes))
                {
                    var recipe = foundRecipes.First();
                    foreach (var resource in recipe.resources)
                    {
                        if (resources.TryGetValue(resource.id, out var amount))
                        {
                            var remaining = amount - resource.amount;
                            if (remaining > 0)
                                resources[resource.id] = remaining;
                            else
                                resources.Remove(resource.id);

                            Jotunn.Logger.LogInfo($"ToDoResources: Removed Resource {resource.id} amount {amount} - left {remaining}");
                        }
                    }

                    foundRecipes.RemoveAt(0);
                    if (foundRecipes.Count == 0)
                        recipes.Remove(id);
                }
            }
        }
    }
}
