//using UnityEngine.Assertions;
using HarmonyLib;
using System.Collections.Generic;
using Xunit;

namespace ValheimModToDo
{
    public class ToDoResoucesTest
    {
        static readonly string test_recipe_id_1 = "Recipe_1";
        static readonly string test_recipe_id_2 = "Recipe_2";

        static readonly string test_resource_id_1 = "$item_1";
        static readonly string test_resource_id_2 = "$item_2";
        static readonly string test_resource_id_3 = "$item_3";

        static ToDoRecipe test_recipe_1;
        static ToDoRecipe test_recipe_2;

        public ToDoResoucesTest()
        {
            test_recipe_1 = new ToDoRecipe(test_recipe_id_1, "Recipe One");
            test_recipe_1.resources.Add(new ToDoResource(test_resource_id_1, "Resource One", 1));
            test_recipe_1.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 5));

            test_recipe_2 = new ToDoRecipe(test_recipe_id_2, "Recipe Two");
            test_recipe_2.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 3));
            test_recipe_2.resources.Add(new ToDoResource(test_resource_id_3, "Resource Three", 9));
        }

        [Fact]
        public void AddSingleRecipeTest()
        {
            var todo = new ToDoResources();
            todo.AddRecipe(test_recipe_1);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 5);
        }

        [Fact]
        public void AddDuplicateRecipeTest()
        {
            var todo = new ToDoResources();
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_1);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 2);
            Assert.Equal(todo.resources[test_resource_id_2], 10);
        }

        [Fact]
        public void AddMultipleRecipesTest()
        {
            var todo = new ToDoResources();
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_2);

            Assert.Equal(todo.resources.Count, 3);
            Assert.Equal(todo.recipes.Count, 2);
            Assert.Equal(todo.resources[test_resource_id_1], 2);
            Assert.Equal(todo.resources[test_resource_id_2], 13);
            Assert.Equal(todo.resources[test_resource_id_3], 9);
        }

        [Fact]
        public void RemoveRecipesTest()
        {
            var todo = new ToDoResources();
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_2);

            todo.RemoveRecipe(test_recipe_1.id);

            Assert.Equal(todo.resources.Count, 3);
            Assert.Equal(todo.recipes.Count, 2);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 8);
            Assert.Equal(todo.resources[test_resource_id_3], 9);

            todo.RemoveRecipe(test_recipe_2.id);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 5);

            todo.RemoveRecipe(test_recipe_1.id);

            Assert.Equal(todo.resources.Count, 0);
            Assert.Equal(todo.recipes.Count, 0);
        }

        [Fact]
        public void RemoveUnknownRecipe()
        {
            var todo = new ToDoResources();
            todo.RemoveRecipe("unknown-recipe");

            Assert.Equal(todo.resources.Count, 0);
            Assert.Equal(todo.recipes.Count, 0);
        }

        [Fact]
        public void WriteAndReadFile()
        {
            var todo = new ToDoResources();
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_1);
            todo.AddRecipe(test_recipe_2);

            todo.fakeRecipeDb = new Dictionary<string, ToDoRecipe>
            {
                { test_recipe_1.id, test_recipe_1 },
                { test_recipe_2.id, test_recipe_2 }
            };

            Assert.Equal(todo.resources.Count, 3);
            Assert.Equal(todo.recipes.Count, 2);
            Assert.Equal(todo.resources[test_resource_id_1], 2);
            Assert.Equal(todo.resources[test_resource_id_2], 13);
            Assert.Equal(todo.resources[test_resource_id_3], 9);

            todo.SaveToFile();

            todo.ClearRecipes();

            todo.LoadFromFile();

            Assert.Equal(todo.resources.Count, 3);
            Assert.Equal(todo.recipes.Count, 2);
            Assert.Equal(todo.resources[test_resource_id_1], 2);
            Assert.Equal(todo.resources[test_resource_id_2], 13);
            Assert.Equal(todo.resources[test_resource_id_3], 9);
        }
    }
}