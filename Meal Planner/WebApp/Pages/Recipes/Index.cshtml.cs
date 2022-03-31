#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.Domain;

namespace WebApp.Pages_Recipes
{
    public class IndexModel : PageModel
    {
        private readonly WebApp.DAL.AppDbContext _context;

        public IndexModel(WebApp.DAL.AppDbContext context)
        {
            _context = context;
        }

        public IList<Recipe> Recipe { get;set; }

        public string? SearchByCategory { get; set; }

        public async Task OnGetAsync(string? searchByCategory, string? action)
        {
            if (action == "Clear")
            {
                searchByCategory = null;
            }

            SearchByCategory = searchByCategory;
            
            Recipe = await _context.Recipes.ToListAsync();
            
            if (!string.IsNullOrWhiteSpace(searchByCategory))
            {
                var search = CreateArray(searchByCategory);

                IList<Recipe> recipes = new List<Recipe>();
                for (int i = 0; i < search.Length; i++)
                {
                    foreach (var recipe in _context.Recipes)
                    {
                        if (recipe.Category.ToUpper() == search[i].ToUpper())
                        {
                            recipes.Add(recipe);
                        }
                    }
                }

                Recipe = recipes;
            }
        }
        
        private string[] CreateArray(string search)
        {
            var searchIngredients = search.Trim().Split(",");
            for (var i = 0; i < searchIngredients.Length; i++)
            {
                searchIngredients[i] = searchIngredients[i].Trim();
            }
            return searchIngredients;
        }
    }
}
