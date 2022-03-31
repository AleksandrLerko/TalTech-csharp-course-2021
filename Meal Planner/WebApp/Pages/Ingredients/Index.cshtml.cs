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

namespace WebApp.Pages_Ingredients
{
    public class IndexModel : PageModel
    {
        private readonly WebApp.DAL.AppDbContext _context;

        public IndexModel(WebApp.DAL.AppDbContext context)
        {
            _context = context;
        }

        public IList<Ingredient> Ingredient { get;set; }

        
        public string? SearchByLocation { get; set; }

        public async Task OnGetAsync(string? searchByLocation, string? action)
        {
            if (action == "Clear")
            {
                searchByLocation = null;
            }
            SearchByLocation = searchByLocation;
            Ingredient = await _context.Ingredients.ToListAsync();
            if (!string.IsNullOrWhiteSpace(searchByLocation))
            {
                var search = CreateArray(searchByLocation);

                IList<Ingredient> ingredients = new List<Ingredient>();
                for (int i = 0; i < search.Length; i++)
                {
                    foreach (var ingredient in _context.Ingredients)
                    {
                        if (ingredient.Location.ToUpper() == search[i].ToUpper())
                        {
                            ingredients.Add(ingredient);
                        }
                    }
                }

                Ingredient = ingredients;
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
