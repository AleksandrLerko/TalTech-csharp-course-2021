using System.Text.Json;
using BattleShipBrain;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Helpers;

namespace WebApp.Pages.Game;

public class Create : PageModel
{
    private readonly DAL.ApplicationDbContext _context;
    
    public Create(DAL.ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public GameConfig GameConfig { get; set; } = new()
    {
        BoardSizeX = 10,
        BoardSizeY = 10
    };

    public List<string> Message { get; set; } = new();

    [BindProperty] public string TouchRule { get; set; } = default!;

    public List<string> TouchRules = new List<string>()
    {
        "NoTouch",
        "CornerTouch",
        "SideTouch"
    };

    public IActionResult OnGet()
    {
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (GameConfig.BoardSizeX < 10)
        {
            Message.Add("BoardSizeX value should be 10 or bigger.");
        }
            
        if (GameConfig.BoardSizeY < 10)
        {
            Message.Add("BoardSizeY value should be 10 or bigger.");
        }   

        if (Message.Count > 0)
        {
            return Page();
        }
            
            
        if (!ModelState.IsValid)
        {
            return Page();
        }

        //_context.GameConfigs.Add(GameConfig);
        //await _context.SaveChangesAsync();

        var getShipConf = SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "ShipConfigEdit");
        EShipTouchRule touchRule;
        switch (TouchRule)
        {
            case "NoTouch":
                touchRule = EShipTouchRule.NoTouch;
                break;
            case "CornerTouch":
                touchRule = EShipTouchRule.CornerTouch;
                break;
            case "SideTouch":
                touchRule = EShipTouchRule.SideTouch;
                break;
            default:
                touchRule = EShipTouchRule.NoTouch;
                break;
        }
        var shConfObj = new GameConfigBrain()
        {
            GameName = GameConfig.GameName,
            BoardSizeX = GameConfig.BoardSizeX,
            BoardSizeY = GameConfig.BoardSizeY,
            EShipTouchRule = touchRule,
            ShipConfigs = IsExist(getShipConf) ? getShipConf : new List<ShipConfigBrain>()
        };
        if (shConfObj.ShipConfigs!.Count == 0)
        {
            shConfObj.CreateDefaultSetup();
        }
        /*
        var jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        */
        
        //var gameConfigBrainJsonData = JsonSerializer.Serialize(shConfObj, jsonOptions);
        SessionHelper.SetObjectAsJson(HttpContext.Session, GameConfig.GameName, shConfObj);
        
        //var path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "ConfigSaves" + Path.DirectorySeparatorChar + GameConfig.GameName + ".json";
        //Console.WriteLine(path);
        //await System.IO.File.WriteAllTextAsync(path, gameConfigBrainJsonData);
        var url = Url.Page("./Index", new {GameName=GameConfig.GameName, playerId=0});
        //var url = Url.Page("./Index", new {gameConfig=GameConfig});

        //return RedirectToPage($"/Game?gameName={GameConfig!.GameName}&xSize={GameConfig.BoardSizeX}&ySize={GameConfig.BoardSizeY}");
        //Console.WriteLine(_gameCount);
        return Redirect(url!);
    }
    
    private bool IsExist(List<ShipConfigBrain>? element)
    {
        return element != null;
    }
}