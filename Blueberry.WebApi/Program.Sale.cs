using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.WebRequestMethods;
static partial class Program
{
    internal static void MapSaleEndpoints(this WebApplication app)
    {
        app.MapGet("/api/Sales/{merchantId}", async (int merchantId, BlueberryContext db) =>
        {
            var sales = await db.Sales
                .Include(s => s.SaleItems)
                .Where(s => s.MerchantId == merchantId)
                .Select(m => new
                {
                    m.Id,
                    m.Date,
                    m.Total,
                    m.TransNumber,
                    m.AuthNumber,
                    m.MerchantId,

                    SaleItems = m.SaleItems.Select(si => new
                    {
                        si.Id,
                        si.SaleId,
                        si.ProductId,
                        si.Quantity,
                        si.UnitPrice,
                        si.Tpsamount,
                        si.Tvqamount
                    }),
                })
                .ToArrayAsync();

            return Results.Ok(sales);
            /*app.MapGet("/api/MerchantProducts/{merchantId}", async (BlueberryContext db, int merchantId) =>
            {
                return await db.MerchantProducts
                    .Where(mp => mp.MerchantId == merchantId)
                    .ToArrayAsync();*/
        });

        app.MapPost("/api/Sales/{merchantId}", async (Blueberry.EntityModels.CreateSaleRequest request, int merchantId, BlueberryContext db) =>
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {
                var sale = new Sale
                {
                    Date = request.Date,
                    Total = request.Total,
                    TransNumber = $"TX-{request.Date:yyyyMMddHHmmss}",
                    AuthNumber = GenerateAuthorizationCode(),
                    MerchantId = merchantId
                };

                db.Sales.Add(sale);

                await db.SaveChangesAsync();

                var saleId = sale.Id;

                foreach (var item in request.Items)
                {
                    var merchantProduct = await db.MerchantProducts
                        .FirstOrDefaultAsync(mp =>
                        mp.MerchantId == merchantId &&
                        mp.ProductId == item.ProductId);
                    if (merchantProduct is null)
                        return Results.NotFound("Produit introuvable chez ce marchand.");

                    merchantProduct.Stock -= item.Quantity;

                    var saleItem = new SaleItem
                    {
                        SaleId = saleId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.ItemPrice,
                        Tpsamount = item.TpsAmount,
                        Tvqamount = item.TvqAmount
                    };

                    db.SaleItems.Add(saleItem);
                }

                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return Results.Ok(sale.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
    static string GenerateAuthorizationCode()
    {
        var random = new Random();
        return $"AUTH-{random.Next(1000, 9999)}";
    }
}

