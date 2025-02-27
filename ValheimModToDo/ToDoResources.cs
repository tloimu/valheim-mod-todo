using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Xunit.Sdk;

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
            name = Localization.instance.Localize(id);
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
            name = Localization.instance.Localize(piece.m_name);
            quality = 1;
            foreach (var resource in piece.m_resources)
            {
                resources.Add(new ToDoResource(resource, quality));
            }
        }

        public ToDoRecipe(Recipe recipe, int quality)
        {
            id = recipe.name;
            this.quality = quality;
            name = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
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

        static public string GetRecipeKey(Recipe recipe, int quality)
        {
            return GetRecipeKey(recipe.name, quality);
        }

        static public string GetRecipeKey(ToDoRecipe recipe)
        {
            return GetRecipeKey(recipe.id, recipe.quality);
        }

        static public string GetRecipeKey(string name, int quality)
        {
            if (quality == 1)
                return name;
            else
                return $"{name}/upgrade";
        }
    }

    public class ToDoResources
    {
        public Dictionary<string, List<ToDoRecipe>> recipes = new();
        public Dictionary<string, int> resources = new();
        public string notes = "";

        private readonly object _recipeLock = new();
        private bool wasChangedSince= false;

        public void SetNotes(string notes)
        {
            if (notes == null)
                notes = "";

            if (!this.notes.Equals(notes))
            {
                this.notes = notes;
                wasChangedSince = true;
            }
        }

        public bool WasChangedSince()
        {
            var wasChanged = wasChangedSince;
            wasChangedSince = false;
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

        public void AddRecipe(Recipe recipe, int quality)
        {
            AddRecipe(new ToDoRecipe(recipe, quality));
        }

        public void AddRecipe(Piece piece)
        {
            AddRecipe(new ToDoRecipe(piece));
        }

        public void AddRecipe(ToDoRecipe recipe)
        {
            var recipeKey = ToDoRecipe.GetRecipeKey(recipe);
            Jotunn.Logger.LogInfo($"ToDoResources: AddRecipe({recipeKey})");
            lock (_recipeLock)
            {
                if (recipes.TryGetValue(recipeKey, out var list))
                {
                    list.Add(recipe);
                    Jotunn.Logger.LogInfo($"ToDoResources: Added Recipe {recipeKey}");
                }
                else
                {
                    recipes.Add(recipeKey, new List<ToDoRecipe> { recipe });
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
                wasChangedSince = true;
            }
        }

        public void RemoveRecipe(Recipe recipe, int quality)
        {
            RemoveRecipe(ToDoRecipe.GetRecipeKey(recipe, quality));
        }

        public void RemoveRecipe(string name, int quality)
        {
            RemoveRecipe(ToDoRecipe.GetRecipeKey(name, quality));
        }

        public void RemoveRecipe(string recipeKey)
        {
            Jotunn.Logger.LogInfo($"ToDoResources: RemoveRecipe({recipeKey})");
            lock (_recipeLock)
            {
                if (recipes.TryGetValue(recipeKey, out var foundRecipes))
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
                        recipes.Remove(recipeKey);
                    wasChangedSince = true;
                }
            }
        }

        public Dictionary<string, ToDoRecipe> fakeRecipeDb;

        public bool IsUnitTesting() { return fakeRecipeDb != null; }

        public Recipe FindRecipe(string name)
        {
            var allRecipes = ObjectDB.instance.m_recipes;
            foreach (var r in allRecipes)
            {
                if (name == r.name)
                    return r;
            }
            return null;
        }

        public Piece FindPiece(string name)
        {
            if (Player.m_localPlayer != null)
            {
                foreach (var pl in Player.m_localPlayer.m_buildPieces.m_availablePieces)
                {
                    foreach (var p in pl)
                    {
                        if (p.name == name)
                            return p;
                    }
                }
            }

            return null;
        }


        public bool LoadFromFile()
        {
            var fileName = GetSaveFileName();
            Jotunn.Logger.LogInfo($"ToDoResources: LoadFromFile({fileName})");
            if (fileName == null)
                return false;

            if (File.Exists(fileName))
            {
                ClearRecipes();
                try
                {
                    var text = File.ReadAllText(fileName);
                    if (text != null)
                    {
                        var serializer = new XmlSerializer(typeof(RecipesList));
                        var fs = new FileStream(fileName, FileMode.Open);
                        var saveRecipes = (RecipesList)serializer.Deserialize(fs);
                        foreach (var saveRecipe in saveRecipes.recipes)
                        {
                            if (IsUnitTesting())
                                AddSavedRecipeInUnitTest(saveRecipe.id, saveRecipe.quality);
                            else
                                AddSavedRecipeInValheim(saveRecipe.id, saveRecipe.quality);
                        }
                        notes = saveRecipes.notes;
                        fs.Close();
                    }
                }
                catch (IOException e)
                {
                    Jotunn.Logger.LogError($"ToDoResources: LoadFromFile({fileName}) exception:{e.Message}");
                    return false;
                }
            }
            return true;
        }


        public void AddSavedRecipeInUnitTest(string name, int quality)
        {
            fakeRecipeDb.TryGetValue(name, out var recipe);
            if (recipe != null)
            {
                recipe.quality = quality;
                AddRecipe(recipe);
            }
        }

        public void AddSavedRecipeInValheim(string name, int quality)
        {
            var recipe = FindRecipe(name);
            if (recipe != null)
                AddRecipe(recipe, quality);
            else
            {
                var piece = FindPiece(name);
                if (piece != null)
                    AddRecipe(piece);
                else
                    Jotunn.Logger.LogWarning($"Loading saved to-do-list: Unable to find recipe or piece [{name}]");
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
            saveRecipes.notes = notes;
            var serializer = new XmlSerializer(typeof(RecipesList));
            TextWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, saveRecipes);
            writer.Close();
        }

        public string GetSaveFileName()
        {
            if (IsUnitTesting())
                return GetSaveFileNameInUnitTest();
            else
                return GetSaveFileNameInValheim();
        }

        public string GetSaveFileNameInUnitTest()
        {
            var playerName = "test";
            var mapName = "map";
            var fileName = $"todo-list-for-{playerName}-in-{mapName}-v1.xml";
            return fileName;
        }
        public string GetSaveFileNameInValheim()
        {
            var mapName = ZNet.instance?.GetWorldName();
            var playerName = Player.m_localPlayer?.GetPlayerName();
            Jotunn.Logger.LogInfo($"ToDoResources: GetFileName for [{playerName}] in [{mapName}])");
            if (playerName == null)
                return null;
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
        public string notes;
    }

    public class recipe
    {
        [XmlAttribute]
        public string id = "";
        [XmlAttribute]
        public int quality = 1;
    }
}
