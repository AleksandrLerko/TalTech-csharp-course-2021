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
dotnet ef migrations add Initial --project WebApp --startup-project WebApp
~~~

Update (apply) DB
~~~
dotnet ef database update --project WebApp --startup-project WebApp 
~~~

Delete DB
~~~
dotnet ef database drop --project WebApp --startup-project WebApp 
~~~

WebApp
~~~
Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore - AddDatabaseDeveloperPageExceptionFilter
Microsoft.VisualStudio.Web.CodeGeneration.Design 
~~~

Razor Pages
~~~
dotnet aspnet-codegenerator razorpage -m Ingredient -dc AppDbContext -udl -outDir Pages/Ingredients --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m Recipe -dc AppDbContext -udl -outDir Pages/Recipes --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m RecipeIngredient -dc AppDbContext -udl -outDir Pages/RecipeIngredients --referenceScriptLibraries -f
dotnet aspnet-codegenerator razorpage -m Product -dc AppDbContext -udl -outDir Pages/Products --referenceScriptLibraries -f
~~~

https://getbootstrap.com/docs/4.6/layout/overview/