using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Game;

public class LoadGame : PageModel
{
    private readonly DAL.ApplicationDbContext _context;
    
    public LoadGame(DAL.ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public List<string> GameSaveNames { get; set; } = default!;

    [BindProperty] public string CurrentSave { get; set; } = default!;

    [BindProperty]
    public string SaveType { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string type)
    {
        Console.WriteLine(type);
        switch (type)
        {
            case "json":
                SaveType = type;
                var gameFiles = Directory
                    .EnumerateFiles(
                        Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GameHistory" +
                        Path.DirectorySeparatorChar, "*.json").ToList();
                GameSaveNames = gameFiles;
                foreach (var gameFile in gameFiles)
                {
                    Console.WriteLine(gameFile);
                }
                break;
            case "db":
                SaveType = type;
                GameSaveNames = _context.GameConfigs.Select(x => x.GameName).ToList();
                break;
        }
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        //Console.WriteLine(CurrentSave);
        var url = "";
        if (SaveType.Equals("json"))
        {
            url = Url.Page("./Index", new {save=CurrentSave, type="json"});
        }
        else
        {
           url = Url.Page("./Index", new {save=CurrentSave, type="db"});
        }
        return Redirect(url!);
    }
}