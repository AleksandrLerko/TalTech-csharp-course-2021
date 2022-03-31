using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using WebApp.DAL;
using WebApp.Domain;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    
    private readonly AppDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public IList<Recipe> Recipe { get; set; } = default!;

    //public int Amount { get; set; }
    public string? SearchName { get; set; }
    
    public string? SearchTime { get; set; }
    public string? SearchIngredientPositive { get; set; }
    public string? SearchIngredientNegative { get; set; }

    public string? ErrorMessage { get; set; } = "";

    public async Task OnGetAsync(string? searchName, string? searchIngredientPositive, string? searchIngredientNegative, string? searchTime, string? action, string? amountToChange, string? productId, string? time)
    {
        Recipe = await _context.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(x => x.Ingredient)
            .ToListAsync();

        if (action == "Clear")
        {
            searchName = null;
            searchIngredientPositive = null;
            searchIngredientNegative = null;
        }
        
        SearchName = searchName;
        SearchIngredientPositive = searchIngredientPositive;
        SearchIngredientNegative = searchIngredientNegative;
        SearchTime = searchTime;

        if (action == "Create")
        {
            Console.WriteLine("dsa");
            Calculations(int.Parse(productId), int.Parse(amountToChange), int.Parse(time));
        }

        if (time == null)
        {
            Calculations(null, null, null);
        }
        
        
        IList<Recipe> tempRecipeList = new List<Recipe>();
        var IsValid = false;
        var id = 0;
        var index = 0;
        foreach (var recipe in _context.Recipes)
        {
            IsValid = false;
            if (id != recipe.Id)
            {
                var value = 0.0;
                foreach (var recipeIngredient in recipe.RecipeIngredients!)
                {
                    var ingredientId = recipeIngredient.IngredientId;
                    var quantity = recipeIngredient.Qty;
                    var neededIngredient = _context.Ingredients.FirstOrDefault(x => x.Id == ingredientId);
                    var amountStr = neededIngredient!.Amount;
                    var amountStrTemp = "";
                    for (int i = 0; i < amountStr.Length - 1; i++)
                    {
                        if (!Regex.IsMatch(amountStr[i].ToString(), "[a-z]", RegexOptions.IgnoreCase))
                        {
                            amountStrTemp += amountStr[i].ToString();
                        }
                    }

                    var amount = float.Parse(amountStrTemp);
                    if (amount >= quantity)
                    {
                        if (value == 0.0 || value > amount / quantity)
                        {
                            value = amount / quantity; 
                        }
                        IsValid = true;
                    }
                    else
                    {
                        IsValid = false;
                    }
                
                }
                if (IsValid)
                {
                    recipe.Servings = (int) value;
                    tempRecipeList.Add(recipe);
                }   
            }
            id = recipe.Id;
            index++;
        }

        Recipe = tempRecipeList;
        await _context.SaveChangesAsync();
        
        var recipeQuery = _context.Recipes
            .Include(x => x.RecipeIngredients!)
            .ThenInclude(x => x.Ingredient)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            searchName = searchName.Trim();
            recipeQuery = recipeQuery.Where(r =>
                r.RecipeName.ToUpper().Contains(searchName.ToUpper()));
        }
        
        if (!string.IsNullOrWhiteSpace(searchIngredientPositive))
        {
            Console.WriteLine("postitive");
            var searchIngredients = CreateArray(searchIngredientPositive);
            // get data from db to memory
            Recipe = await recipeQuery.ToListAsync();
            
            // filter it in memory
            Recipe = Recipe.Where(r =>
                r.RecipeIngredients.Any(i =>
                    searchIngredients.Any(s => i.Ingredient!.IngredientName.ToUpper().Contains(s.ToUpper())))).ToList();
        }
        
        if (!string.IsNullOrWhiteSpace(searchIngredientNegative))
        {
            Console.WriteLine("negative");
            var searchIngredients = CreateArray(searchIngredientNegative);
            // get data from db to memory
            Recipe = await recipeQuery.ToListAsync();

            IList<Recipe> tempRecipe = new List<Recipe>();

            var IsValidFilter = true;

            // filter it in memory
            foreach (var recipe in _context.Recipes)
            {
                IsValidFilter = true;
                foreach (var recipeIngredient in recipe.RecipeIngredients!)
                {
                    var ingredientId = recipeIngredient.IngredientId;
                    var findIngredient = _context.Ingredients.FirstOrDefault(x => x.Id == ingredientId);
                    for (int i = 0; i < searchIngredients.Length; i++)
                    {
                        //searchIngredients.con
                        if (findIngredient!.IngredientName.ToUpper() == searchIngredients[i].ToUpper())
                        {
                            IsValidFilter = false;
                        }
                    }
                }

                if (IsValidFilter)
                {
                    tempRecipe.Add(recipe);
                }
            }

            Recipe = tempRecipe.Count != 0 ? tempRecipe : Recipe;
        }

        if (!string.IsNullOrWhiteSpace(searchTime))
        {
            var searchIngredients = CreateArray(searchTime);
            // get data from db to memory
            Recipe = await recipeQuery.ToListAsync();
            
            Recipe = Recipe.Where(r => r.TimeNeeded <= int.Parse(searchIngredients[0])).ToList();
        }

    }

    private string[] CreateArray(string searchIngredientNegative)
    {
        var searchIngredients = searchIngredientNegative.Trim().Split(",");
        for (var i = 0; i < searchIngredients.Length; i++)
        {
            searchIngredients[i] = searchIngredients[i].Trim();
        }
        return searchIngredients;
    }

    private void Calculations(int? id, int? amount, int? time)
    {
        foreach (var recipe in Recipe)
        {

            if (recipe.Id == id)
            {
                if (recipe.TimeNeeded <= time)
                {
                    foreach (var recipeIngredient in recipe.RecipeIngredients!)
                    {
                        var calculatedAmount = recipeIngredient.Qty * amount;
                        var ingredientId = recipeIngredient.IngredientId;
                        var neededIngredient = _context.Ingredients.FirstOrDefault(x => x.Id == ingredientId);
                        var amountStr = neededIngredient!.Amount;
                        var amountStrTemp = "";
                        var units = "";
                        for (int i = 0; i < amountStr.Length; i++)
                        {
                            if (!Regex.IsMatch(amountStr[i].ToString(), "[a-z]", RegexOptions.IgnoreCase))
                            {
                                amountStrTemp += amountStr[i].ToString();
                            }
                            else
                            {
                                units += amountStr[i].ToString();
                            }
                        }

                        var ingredientsAmount = float.Parse(amountStrTemp);
                        if (ingredientsAmount >= calculatedAmount)
                        {
                            var calculated = ingredientsAmount - calculatedAmount;
                            _context.Ingredients.FirstOrDefault(x => x.Id == ingredientId)!.Amount =
                                calculated.ToString() + units;
                            _context.SaveChanges();
                        }
                    }

                    recipe.Servings--;
                }
                else
                {
                    ErrorMessage = "Not enough time to prepare " + recipe.RecipeName;
                }
            }
        }
    }
}