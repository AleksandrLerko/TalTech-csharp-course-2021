using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain;

public class Recipe: BaseEntity
{
    [MaxLength(128)]
    public string RecipeName { get; set; } = default!;

    public int Servings { get; set; }

    [MaxLength(128)]
    public string Category { get; set; } = default!;

    public int TimeNeeded { get; set; }
    
    public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
}