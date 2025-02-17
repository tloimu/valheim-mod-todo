using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ValheimModToDo
{
    public class ToDoResource
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

    public class ToDoRecipe
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


    public class ToDoResources
    {
        public Dictionary<string, List<ToDoRecipe>> recipes = new();
        public Dictionary<string, int> resources = new();

        private readonly object _recipeLock = new();
        private bool hasRecipeListChanged = false;

        public bool HasRecipeListChanged()
        {
            var wasChanged = hasRecipeListChanged;
            hasRecipeListChanged = false;
            return wasChanged;
        }

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
                hasRecipeListChanged = true;
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
                    hasRecipeListChanged = true;
                }
            }
        }

        public Dictionary<string, ToDoRecipe> fakeRecipeDb;

        public Recipe FindRecipe(string id)
        {
            var allRecipes = ObjectDB.instance.m_recipes;
            foreach (var r in allRecipes)
            {
                var recipeId = ToDoRecipe.GetRecipeId(r);
                if (id == recipeId)
                    return r;
            }
            return null;
        }

        public void LoadFromFile()
        {
            var fileName = GetSaveFileName();
            Jotunn.Logger.LogInfo($"ToDoResources: LoadFromFile({fileName})");

            if (File.Exists(fileName))
            {
                var text = File.ReadAllText(fileName);
                if (text != null)
                {
                    ClearRecipes();

                    var serializer = new XmlSerializer(typeof(RecipesList));
                    var fs = new FileStream(fileName, FileMode.Open);
                    var saveRecipes = (RecipesList)serializer.Deserialize(fs);
                    foreach (var saveRecipe in saveRecipes.recipes)
                    {
                        if (fakeRecipeDb != null)
                        {
                            fakeRecipeDb.TryGetValue(saveRecipe.id, out var recipe);
                            if (recipe != null)
                                AddRecipe(recipe);
                        }
                        else
                        {
                            var recipe = FindRecipe(saveRecipe.id);
                            if (recipe != null)
                                AddRecipe(recipe);
                        }
                    }
                    fs.Close();
                }
            }
        }

        public void SaveToFile()
        {
            var fileName = GetSaveFileName();
            Jotunn.Logger.LogInfo($"ToDoResources: SaveToFile({fileName})");
            var saveRecipes = new RecipesList();
            foreach (var recipeEntry in recipes)
            {
                foreach (var recipe in recipeEntry.Value)
                {
                    var saveRecipe = new recipe
                    {
                        id = recipe.id,
                        quality = recipe.quality
                    };
                    saveRecipes.recipes.Add(saveRecipe);
                }
            }
            var serializer = new XmlSerializer(typeof(RecipesList));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, saveRecipes);
            writer.Close();
        }

        public string GetSaveFileName()
        {
            var playerName = Player.m_localPlayer.GetPlayerName();
            var mapName = "map";
            var fileName = $"todo-list-for-{playerName}-in-{mapName}-v1.xml";
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }


    [XmlRootAttribute("todolist")]
    public class RecipesList
    {
        [XmlAttribute]
        public string version = "1";
        public List<recipe> recipes = new();
    }

    public class recipe
    {
        [XmlAttribute]
        public string id = "";
        [XmlAttribute]
        public int quality = 1;
    }
}
