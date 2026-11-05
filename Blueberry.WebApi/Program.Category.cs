using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;

static partial class Program
{
    internal static void MapCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/Categories", async (BlueberryContext db) =>
        {
            return await db.Categories.ToArrayAsync();
        });

        app.MapGet("/api/Categories/{id}", async (int id, BlueberryContext db) =>
        {
            var category = await db.Categories.FindAsync(id);
            return category is not null ? Results.Ok(category) : Results.NotFound();
        });

        app.MapPost("/api/Categories", async (Category category, BlueberryContext db) =>
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return Results.BadRequest("Le nom de la catégorie est obligatoire.");
            }

            var newCategory = new Category
            {
                Name = category.Name.Trim(),
                Tps = category.Tps,
                Tvq = category.Tvq,
                IsActive = true
            };

            db.Categories.Add(newCategory);
            await db.SaveChangesAsync();

            return Results.Created($"/api/Categories/{newCategory.Id}", newCategory);
        });

        app.MapPut("/api/Categories/{id}", async (int id, Category category, BlueberryContext db) =>
        {
            var existingCategory = await db.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return Results.BadRequest("Le nom de la catégorie est obligatoire.");
            }

            existingCategory.Name = category.Name.Trim();
            existingCategory.Tps = category.Tps;
            existingCategory.Tvq = category.Tvq;

            await db.SaveChangesAsync();
            return Results.Ok(existingCategory);
        });

        app.MapPut("/api/Categories", async (Category category, BlueberryContext db) =>
        {
            var existingCategory = await db.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return Results.BadRequest("Le nom de la catégorie est obligatoire.");
            }

            existingCategory.Name = category.Name.Trim();
            existingCategory.Tps = category.Tps;
            existingCategory.Tvq = category.Tvq;

            await db.SaveChangesAsync();
            return Results.Ok(existingCategory);
        });

        app.MapDelete("/api/Categories/{id}", async (int id, BlueberryContext db) =>
        {
            var category = await db.Categories
                .Include(c => c.Products)
                .Include(c => c.Merchants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Results.NotFound();
            }

            if (category.Products.Any())
            {
                var activeProducts = category.Products.Where(p => p.IsActive).ToList();
                if (activeProducts.Any())
                { 
                    //return Results.BadRequest("Impossible de supprimer cette catégorie car elle contient des produits associés.");
                    return Results.Ok(new Blueberry.EntityModels.SqlServer.DTOs.DeleteProductResponse
                    {
                        Message = "Impossible de supprimer cette catégorie car elle contient des produits associés."
                    });
                }
                else
                {
                    category.IsActive = false;
                    await db.SaveChangesAsync();

                    return Results.Ok(new Blueberry.EntityModels.SqlServer.DTOs.DeleteProductResponse
                    {
                        Message = "La category a été désactivé car tous ses produits sont inactifs."
                    });
                }
            }

            category.Merchants.Clear();
            db.Categories.Remove(category);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
