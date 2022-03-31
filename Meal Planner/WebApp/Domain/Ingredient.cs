using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain;

public class Ingredient: BaseEntity
{
    [MaxLength(128)]
    public string IngredientName { get; set; } = default!;

    [MaxLength(128)]
    public string Location { get; set; } = default!;
    
    [MaxLength(128)]
    public string Category { get; set; } = default!;

    [MaxLength(128)] public string Amount { get; set; } = default!;
    
    public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
}