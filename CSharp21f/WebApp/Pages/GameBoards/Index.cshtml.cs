using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleShipBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebApp.Pages_GameBoards
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SaveGameDTO.GameBoardDTO?> GameBoard { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var iwq = new List<SaveGameDTO.GameBoardDTO?>();
            foreach (var board in _context.GameBoards)
            {
                 iwq.Add(JsonSerializer.Deserialize<SaveGameDTO.GameBoardDTO>(board.BoardData));
                
            }

            GameBoard = iwq;

        }
    }
}
