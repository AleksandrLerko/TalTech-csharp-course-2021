using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleShipBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;
using WebApp.Helpers;

namespace WebApp.Pages_ShipConfigs
{
    public class CreateModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public CreateModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public ShipConfigBrain ShipConfig { get; set; } = default!;

        [BindProperty]
        public string Action { get; set; } = default!;

        [BindProperty]
        public string Submit { get; set; } = default!;

        [BindProperty]
        public string CurrentSelectedShip { get; set; } = default!;

        [BindProperty]
        public int ShipConfSize { get; set; } = default!;
        
        [BindProperty]
        public int ShipConfQuantity { get; set; } = default!;
        
        public List<ShipConfigBrain> ShipConfigs { get; set; } = new()
        {
            new ShipConfigBrain()
            {
                Name = "Patrol",
                Quantity = 5,
                ShipSizeX = 1,
                ShipSizeY = 1,
            },            
            new ShipConfigBrain()
            {
                Name = "Cruiser",
                Quantity = 4,
                ShipSizeX = 1,
                ShipSizeY = 2,
            },            
            new ShipConfigBrain()
            {
                Name = "Submarine",
                Quantity = 3,
                ShipSizeX = 1,
                ShipSizeY = 3,
            },            
            new ShipConfigBrain()
            {
                Name = "Battleship",
                Quantity = 2,
                ShipSizeX = 1,
                ShipSizeY = 4,
            },            
            new ShipConfigBrain()
            {
                Name = "Carrier",
                Quantity = 1,
                ShipSizeX = 1,
                ShipSizeY = 5,
            },
        };
        

        public IActionResult OnGet()
        {
            /*
        ViewData["GameConfigId"] = new SelectList(_context.GameConfigs, "GameConfigId", "GameConfigId");
        */
            Console.WriteLine("get");
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (IsExist(SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "ShipConfigEdit")))
            {
                ShipConfigs =
                    SessionHelper.GetObjectFromJson<List<ShipConfigBrain>>(HttpContext.Session, "ShipConfigEdit")!;
            }
            switch (Submit)
            {
                case "Add ship":
                    ShipConfigs.Add(ShipConfig);
                    break;
                case "Remove ship":
                    ShipConfigs.RemoveAll(x => x.Name.Equals(CurrentSelectedShip));
                    break;
                case "Change size":
                    ShipConfigs.FirstOrDefault(x => x.Name.Equals(CurrentSelectedShip))!.ShipSizeY = ShipConfSize;
                    break;
                case "Change quantity":
                    ShipConfigs.FirstOrDefault(x => x.Name.Equals(CurrentSelectedShip))!.Quantity = ShipConfQuantity;
                    break;
                
            }
            Console.WriteLine(ShipConfigs[0].Name);
            Console.WriteLine(ShipConfigs[0].Quantity);
            Console.WriteLine(ShipConfigs[0].ShipSizeX);
            Console.WriteLine(ShipConfigs[0].ShipSizeY);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "ShipConfigEdit", ShipConfigs);
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("./Index");
        }
        
        private bool IsExist(List<ShipConfigBrain>? element)
        {
            return element != null;
        }
    }
}
