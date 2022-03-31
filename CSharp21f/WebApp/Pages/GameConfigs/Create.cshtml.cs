using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;

namespace WebApp.Pages_GameConfigs
{
    public class CreateModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public CreateModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public GameConfig GameConfig { get; set; } = new()
        {
            BoardSizeX = 10,
            BoardSizeY = 10
        };

        public List<string> Message { get; set; } = new();

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
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

            _context.GameConfigs.Add(GameConfig);
            await _context.SaveChangesAsync();

            return RedirectToPage($"/Game?gameName={GameConfig!.GameName}&xSize={GameConfig.BoardSizeX}&ySize={GameConfig.BoardSizeY}");
        }
    }
}
