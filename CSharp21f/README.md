**DAL.ApplicationDbContext.cs**

For DbContext install:
~~~
Microsoft.EntityFrameworkCore
~~~

OnConfiguring UseSqlServer
~~~
Microsoft.EntityFrameworkCore.SqlServer
~~~

To do migrations, initially install to startup project:
~~~
Microsoft.EntityFrameworkCore.Design
~~~
**Migrations**

Create new migration
~~~
dotnet ef migrations add GameHistory --project DAL --startup-project BattleShipConsoleApp
~~~

Update (apply) DB
~~~
dotnet ef database update --project DAL --startup-project BattleShipConsoleApp 
~~~

Delete DB
~~~
dotnet ef database drop --project DAL --startup-project BattleShipConsoleApp 
~~~

WebApp
~~~
Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore - AddDatabaseDeveloperPageExceptionFilter
Microsoft.VisualStudio.Web.CodeGeneration.Design 
~~~

Razor Pages
~~~
dotnet aspnet-codegenerator razorpage -m GameBoard -dc ApplicationDbContext -udl -outDir Pages/GameBoards --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m GameConfig -dc ApplicationDbContext -udl -outDir Pages/GameConfigs --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m Ship -dc ApplicationDbContext -udl -outDir Pages/Ships --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m ShipConfig -dc ApplicationDbContext -udl -outDir Pages/ShipConfigs --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m ShipQuantity -dc ApplicationDbContext -udl -outDir Pages/ShipQuantities --referenceScriptLibraries -f
~~~

https://getbootstrap.com/docs/4.6/layout/overview/