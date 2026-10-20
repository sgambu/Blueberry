using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

static partial class Program
{
    internal static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/api/Products", async (BlueberryContext db) =>
        {
            return await db.Products.ToArrayAsync();
        });

        app.MapPost("/api/Products", async (Product product, BlueberryContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/Products/{product.Id}", product);
        });

        app.MapPut("/api/Products", async (Product product, BlueberryContext db) =>
        {
            var existingProduct = await db.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                return Results.NotFound();
            }
            existingProduct.Name = product.Name;
            existingProduct.PriceDefault = product.PriceDefault;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

}

