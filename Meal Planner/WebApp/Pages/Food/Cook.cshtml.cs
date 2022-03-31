using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.DAL;
using WebApp.Domain;

namespace WebApp.Pages.Food;

public class Cook : PageModel
{

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public int Amount { get; set; }
    
    [BindProperty]
    public int Time { get; set; }

    public IActionResult OnGet(string? id)
    {
        Id = int.Parse(id!);
        return Page();
    }
    

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {

        var url = Url.Page("/Index",
            new {action = "Create", amountToChange = Amount.ToString(), productId = Id.ToString(), time = Time.ToString()});

        return Redirect(url!);
    }
}