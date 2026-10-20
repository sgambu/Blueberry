using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Text;
using static System.Net.WebRequestMethods;
static partial class Program
{
    internal static void MapMerchantEndpoints(this WebApplication app)
    {

        app.MapGet("/api/Merchants", async (BlueberryContext db) =>
        {
            var merchants = await db.Merchants
                .Include(m => m.Categories)
                .Include(m => m.MerchantProducts)
                .Select(m => new
                {
                    m.Id,
                    m.Adresse,
                    m.Phone,

                    Categories = m.Categories.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Tps,
                        c.Tvq
                    }),

                    MerchantProducts = m.MerchantProducts.Select(mp => new
                    {
                        mp.MerchantId,
                        mp.ProductId,
                        mp.Price,
                        mp.Stock
                    })
                })
                .ToArrayAsync();

            return Results.Ok(merchants);
        });


        /*Une dernière nuance: retourner directement une collection vide avec ToArrayAsync() produit normalement 200 OK a
        voir livre page 745*/
        /*app.MapGet("/api/Merchants/{merchantId}", async (BlueberryContext db, int merchantId) =>
        {
            return await db.Merchants
                .Include(m => m.MerchantProducts)
                .FirstOrDefaultAsync(m => m.Id == merchantId);
        });*/

        app.MapGet("/api/Merchants/{merchantId}", async (int merchantId, BlueberryContext db) =>
        {
            var merchant = await db.Merchants
                .Include(m => m.Categories)
                .Include(m => m.MerchantProducts)
                .FirstOrDefaultAsync(m => m.Id == merchantId);

            if (merchant is null)
                return Results.NotFound();

            return Results.Ok(new
            {
                merchant.Id,
                merchant.Adresse,
                merchant.Phone,

                Categories = merchant.Categories
                    .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Tps,
                    c.Tvq
                }),

                MerchantProducts = merchant.MerchantProducts
                    .Select(mp => new
                {
                    mp.MerchantId,
                    mp.ProductId,
                    mp.Price,
                    mp.Stock
                })
            });
        });

        app.MapPost("/api/Merchants", async (BlueberryContext db, Merchant merchant) =>
        {
            db.Merchants.Add(merchant);
            await db.SaveChangesAsync();
            return Results.Created($"/api/Merchants/{merchant.Id}", merchant);
        });

        app.MapPut("/api/Merchants/{merchantId}", async (BlueberryContext db, int merchantId, Merchant updatedMerchant) =>
        {
            var merchant = await db.Merchants.FindAsync(merchantId);
            if (merchant == null)
            {
                return Results.NotFound();
            }
            merchant.Adresse = updatedMerchant.Adresse;
            merchant.Phone = updatedMerchant.Phone;
            // Update other properties as needed
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapGet("/api/MerchantProducts", async (BlueberryContext db) =>
        {
            return await db.MerchantProducts.ToArrayAsync();
        });

        app.MapGet("/api/MerchantProducts/{merchantId}", async (BlueberryContext db, int merchantId) =>
        {
            return await db.MerchantProducts
                .Where(mp => mp.MerchantId == merchantId)
                .ToArrayAsync();
        });
   
        app.MapPost("/api/MerchantProducts", async (BlueberryContext db, MerchantProduct newProduct) =>
        {
            db.MerchantProducts.Add(newProduct);
            await db.SaveChangesAsync();
            return Results.Created($"/api/MerchantProducts/{newProduct.MerchantId}{newProduct.ProductId}", newProduct);
        });

        /*app.MapPut("/api/MerchantProducts/{merchantId}/{productId}", async (BlueberryContext db, int merchantId, int productId, [FromBody] int quantityToAdd) =>
        {
            var m = await db.MerchantProducts.FirstOrDefaultAsync(mp => mp.MerchantId == merchantId && mp.ProductId == productId);
            if (m == null)
            {
                return Results.NotFound();
            }
            m.Stock += quantityToAdd;
            await db.SaveChangesAsync();
            return Results.Ok();
        });*/

        app.MapPut("/api/MerchantProducts/{merchantId}", async (BlueberryContext db, int merchantId, [FromBody] ProductModification modification) =>
        {
            var m = await db.MerchantProducts.FirstOrDefaultAsync(mp => mp.MerchantId == merchantId && mp.ProductId == modification.ProductId);
            if (m == null)
            {
                return Results.NotFound();
            }
            if (modification.NewPrice != default(decimal))
            {
                m.Price = modification.NewPrice;
            }
            if (modification.QuantityToAdd != default(int) && modification.QuantityToAdd > 0)
            {
                m.Stock += modification.QuantityToAdd;
            }
                
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapGet("/api/Categories", async (BlueberryContext db) =>
        {
                return await db.Categories.ToArrayAsync();
        });

        app.MapGet("/api/MerchantCategory/{merchantId}", async (BlueberryContext db, int merchantId) =>
        {
            var merchantCategories = await db.Merchants
                .Where(mc => mc.Id == merchantId)
                //.Select(mc => mc.MerchantProducts, mc => mc.Categories)
                //.Select(mc => mc.Categories)
                .Select(g => new
                {
                    Categories = g.Categories,
                    MerchantProducts = g.MerchantProducts
                })
                .ToListAsync();
            return Results.Ok(merchantCategories);

        });

        app.MapPost("/api/MerchantCategory/{merchantId}", async (BlueberryContext db, int merchantId, [FromBody] int categoryId) =>
        {
            var merchant = await db.Merchants
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == merchantId);

            var category = await db.Categories.FindAsync(categoryId);

            if (merchant == null || category == null)
            {
                return Results.NotFound();
            }

            // Vérifier si la catégorie existe déjà pour ce marchand
            if (merchant.Categories.Any(c => c.Id == categoryId))
            {
                return Results.Ok();
            }

            merchant.Categories.Add(category);
            await db.SaveChangesAsync();

            return Results.Ok();
        });


    }
}
public class ProductModification
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
    public int QuantityToAdd { get; set; }
}
