//using UnityEngine.Assertions;
using Xunit;

namespace ValheimModToDo
{
    public class ToDoResoucesTest
    {
        static readonly string test_recipe_id_1 = "test_recipe_1";
        static readonly string test_recipe_id_2 = "test_recipe_2";

        static readonly string test_resource_id_1 = "test_resource_1";
        static readonly string test_resource_id_2 = "test_resource_2";
        static readonly string test_resource_id_3 = "test_resource_3";

        [Fact]
        public void AddSingleRecipeTest()
        {
            var todo = new ToDoResources();
            var recipe = new ToDoRecipe(test_recipe_id_1, "Recipe One");
            recipe.resources.Add(new ToDoResource(test_resource_id_1, "Resource One", 1));
            recipe.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 5));
            todo.AddRecipe(recipe);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 5);
        }

        [Fact]
        public void AddDuplicateRecipeTest()
        {
            var todo = new ToDoResources();
            var recipe = new ToDoRecipe(test_recipe_id_1, "Recipe One");
            recipe.resources.Add(new ToDoResource(test_resource_id_1, "Resource One", 1));
            recipe.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 5));
            todo.AddRecipe(recipe);
            todo.AddRecipe(recipe);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 2);
            Assert.Equal(todo.resources[test_resource_id_2], 10);
        }

        [Fact]
        public void AddMultipleRecipesTest()
        {
            var todo = new ToDoResources();
            var recipe1 = new ToDoRecipe(test_recipe_id_1, "Recipe One");
            recipe1.resources.Add(new ToDoResource(test_resource_id_1, "Resource One", 1));
            recipe1.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 5));

            var recipe2 = new ToDoRecipe(test_recipe_id_2, "Recipe Two");
            recipe2.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 3));
            recipe2.resources.Add(new ToDoResource(test_resource_id_3, "Resource Three", 9));
            todo.AddRecipe(recipe1);
            todo.AddRecipe(recipe1);
            todo.AddRecipe(recipe2);

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
            var recipe1 = new ToDoRecipe(test_recipe_id_1, "Recipe One");
            recipe1.resources.Add(new ToDoResource(test_resource_id_1, "Resource One", 1));
            recipe1.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 5));

            var recipe2 = new ToDoRecipe(test_recipe_id_2, "Recipe Two");
            recipe2.resources.Add(new ToDoResource(test_resource_id_2, "Resource Two", 3));
            recipe2.resources.Add(new ToDoResource(test_resource_id_3, "Resource Three", 9));
            todo.AddRecipe(recipe1);
            todo.AddRecipe(recipe1);
            todo.AddRecipe(recipe2);

            todo.RemoveRecipe(recipe1.id);

            Assert.Equal(todo.resources.Count, 3);
            Assert.Equal(todo.recipes.Count, 2);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 8);
            Assert.Equal(todo.resources[test_resource_id_3], 9);

            todo.RemoveRecipe(recipe2.id);

            Assert.Equal(todo.resources.Count, 2);
            Assert.Equal(todo.recipes.Count, 1);
            Assert.Equal(todo.resources[test_resource_id_1], 1);
            Assert.Equal(todo.resources[test_resource_id_2], 5);

            todo.RemoveRecipe(recipe1.id);

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
    }
}