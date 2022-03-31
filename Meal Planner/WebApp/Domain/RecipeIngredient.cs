using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain;

public class RecipeIngredient: BaseEntity
{
    public float Qty { get; set; }

    [MaxLength(4096)]
    public string Comment { get; set; } = default!;

    public int RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public int IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
}