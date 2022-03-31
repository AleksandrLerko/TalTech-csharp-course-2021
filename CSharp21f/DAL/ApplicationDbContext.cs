using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class ApplicationDbContext: DbContext
    {
        private static string ConnectionString =
            "Server=barrel.itcollege.ee;User Id=student;Password=Student.Pass.1;Database=student_allerk_BattleShips;MultipleActiveResultSets=true";
        
        public DbSet<GameBoard> GameBoards { get; set; } = default!;
        public DbSet<GameConfig> GameConfigs { get; set; } = default!;
        public DbSet<Ship> Ship { get; set; } = default!;
        public DbSet<ShipConfig> ShipConfigs { get; set; } = default!;
        public DbSet<ShipQuantity> ShipQuantities { get; set; } = default!;
        public DbSet<GameHistory> GameHistories { get; set; } = default!;
        
        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(
            builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Information);
            });
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // one user (local) ms sql (only on windows) - "Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;"
            optionsBuilder
                .UseLoggerFactory(_loggerFactory)
                .EnableSensitiveDataLogging()
                .UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // remove thew cascade delete globally
            /*
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            */
        }
    }
}