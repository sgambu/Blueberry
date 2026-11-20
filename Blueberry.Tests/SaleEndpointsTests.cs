using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Blueberry.Tests;

public class SaleEndpointsTests
{
    [Fact]
    public async Task GetSalesByMerchantId_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create a merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant For GetSales Test",
            Phone = "555-7777"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // Act
        var response = await client.GetAsync($"/api/Sales/{merchant.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sales = await response.Content.ReadFromJsonAsync<Sale[]>();
        Assert.NotNull(sales);
    }

    [Fact]
    public async Task CreateSale_ShouldReturnSuccess_AndDecrementStock()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create Merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant For Sale Execution",
            Phone = "555-8888"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // 2. Create Category
        var catResponse = await client.PostAsJsonAsync("/api/Categories", new Category
        {
            Name = "Category For Sale Test",
            Tps = true,
            Tvq = true,
            IsActive = true
        });
        var category = await catResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        // 3. Create Product
        var prodResponse = await client.PostAsJsonAsync("/api/Products", new Product
        {
            Name = "Product Sold In Sale",
            PriceDefault = 10.0m,
            CategoryId = category.Id,
            IsActive = true
        });
        var product = await prodResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        // 4. Add product to merchant with initial Stock = 30 and Price = 12.0m
        var addProductResponse = await client.PostAsJsonAsync("/api/MerchantProducts", new MerchantProduct
        {
            MerchantId = merchant.Id,
            ProductId = product.Id,
            Price = 12.0m,
            Stock = 30
        });
        Assert.Equal(HttpStatusCode.Created, addProductResponse.StatusCode);

        // 5. Build CreateSaleRequest
        var saleRequest = new CreateSaleRequest
        {
            Date = DateTime.Now,
            Total = 27.60m,
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    ItemPrice = 12.0m,
                    TpsAmount = 1.20m,
                    TvqAmount = 2.40m
                }
            }
        };

        // Act: Create Sale
        var saleResponse = await client.PostAsJsonAsync($"/api/Sales/{merchant.Id}", saleRequest);

        // Assert: 200 OK with returned sale ID
        Assert.Equal(HttpStatusCode.OK, saleResponse.StatusCode);

        var saleId = await saleResponse.Content.ReadFromJsonAsync<int>();
        Assert.True(saleId > 0);

        // Verify stock has been decremented from 30 to 28 (30 - 2)
        var stockResponse = await client.GetAsync($"/api/MerchantProducts/{merchant.Id}");
        var merchantProducts = await stockResponse.Content.ReadFromJsonAsync<MerchantProduct[]>();
        Assert.NotNull(merchantProducts);

        var updatedStockItem = merchantProducts.FirstOrDefault(mp => mp.ProductId == product.Id);
        Assert.NotNull(updatedStockItem);
        Assert.Equal(28, updatedStockItem.Stock);

        // Verify sale was saved in database
        var getSalesResponse = await client.GetAsync($"/api/Sales/{merchant.Id}");
        Assert.Equal(HttpStatusCode.OK, getSalesResponse.StatusCode);

        var sales = await getSalesResponse.Content.ReadFromJsonAsync<Sale[]>();
        Assert.NotNull(sales);

        var savedSale = sales.FirstOrDefault(s => s.Id == saleId);
        Assert.NotNull(savedSale);
        Assert.Equal(27.60m, savedSale.Total);
        Assert.Single(savedSale.SaleItems);

        var savedItem = savedSale.SaleItems.First();
        Assert.Equal(product.Id, savedItem.ProductId);
        Assert.Equal(2, savedItem.Quantity);
        Assert.Equal(12.0m, savedItem.UnitPrice);
    }

    [Fact]
    public async Task CreateSale_WithProductNotFoundInMerchant_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Create a merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant Empty Stock",
            Phone = "555-9999"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // Attempt to create sale with ProductId that is not in this merchant's inventory
        var saleRequest = new CreateSaleRequest
        {
            Date = DateTime.Now,
            Total = 15.0m,
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = -99999,
                    Quantity = 1,
                    ItemPrice = 15.0m,
                    TpsAmount = 0m,
                    TvqAmount = 0m
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/Sales/{merchant.Id}", saleRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
