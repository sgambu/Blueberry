using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using Blueberry.EntityModels.SqlServer.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Blueberry.Tests;

public class MerchantEndpointsTests
{
    [Fact]
    public async Task GetMerchants_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Merchants");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var merchants = await response.Content.ReadFromJsonAsync<Merchant[]>();
        Assert.NotNull(merchants);
    }

    [Fact]
    public async Task GetMerchantById_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newMerchant = new Merchant
        {
            Adresse = "123 Rue de la Montagne, Montréal",
            Phone = "(514) 555-0101"
        };
        var createResponse = await client.PostAsJsonAsync("/api/Merchants", newMerchant);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(created);

        // Act
        var response = await client.GetAsync($"/api/Merchants/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var merchant = await response.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);
        Assert.Equal(created.Id, merchant.Id);
        Assert.Equal("123 Rue de la Montagne, Montréal", merchant.Adresse);
        Assert.Equal("(514) 555-0101", merchant.Phone);
    }

    [Fact]
    public async Task GetMerchantById_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Merchants/-99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetMerchant_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newMerchant = new Merchant
        {
            Adresse = "456 Boulevard René-Lévesque",
            Phone = "(514) 555-0202"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Merchants", newMerchant);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("456 Boulevard René-Lévesque", created.Adresse);
        Assert.Equal("(514) 555-0202", created.Phone);
    }

    [Fact]
    public async Task UpdateMerchant_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newMerchant = new Merchant
        {
            Adresse = "Initial Address",
            Phone = "555-0000"
        };
        var createResponse = await client.PostAsJsonAsync("/api/Merchants", newMerchant);
        var created = await createResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(created);

        var updatedData = new Merchant
        {
            Adresse = "789 Rue Sainte-Catherine",
            Phone = "(514) 555-0303"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Merchants/{created.Id}", updatedData);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify update
        var getResponse = await client.GetAsync($"/api/Merchants/{created.Id}");
        var merchant = await getResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);
        Assert.Equal("789 Rue Sainte-Catherine", merchant.Adresse);
        Assert.Equal("(514) 555-0303", merchant.Phone);
    }

    [Fact]
    public async Task UpdateMerchant_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var updatedData = new Merchant
        {
            Adresse = "Nowhere",
            Phone = "000-0000"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Merchants/-99999", updatedData);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMerchantProducts_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/MerchantProducts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var merchantProducts = await response.Content.ReadFromJsonAsync<MerchantProduct[]>();
        Assert.NotNull(merchantProducts);
    }

    [Fact]
    public async Task AddMerchantProduct_AndGetByMerchantId_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create a merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant For Product Test",
            Phone = "555-4444"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // 2. Create a category and a product
        var catResponse = await client.PostAsJsonAsync("/api/Categories", new Category
        {
            Name = "Category For Merchant Product",
            Tps = true,
            Tvq = true,
            IsActive = true
        });
        var category = await catResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var prodResponse = await client.PostAsJsonAsync("/api/Products", new Product
        {
            Name = "Product For Merchant",
            PriceDefault = 12.0m,
            CategoryId = category.Id,
            IsActive = true
        });
        var product = await prodResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        // Act 1: POST /api/MerchantProducts
        var newMerchantProduct = new MerchantProduct
        {
            MerchantId = merchant.Id,
            ProductId = product.Id,
            Price = 14.50m,
            Stock = 20
        };
        var addResponse = await client.PostAsJsonAsync("/api/MerchantProducts", newMerchantProduct);

        // Assert 1: Created
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        // Act 2: GET /api/MerchantProducts/{merchantId}
        var getResponse = await client.GetAsync($"/api/MerchantProducts/{merchant.Id}");

        // Assert 2: OK and contains the product
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var products = await getResponse.Content.ReadFromJsonAsync<MerchantProduct[]>();
        Assert.NotNull(products);
        Assert.Contains(products, mp => mp.ProductId == product.Id && mp.Price == 14.50m && mp.Stock == 20);
    }

    [Fact]
    public async Task UpdateMerchantProduct_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant Update Stock",
            Phone = "555-5555"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // 2. Create category & product
        var catResponse = await client.PostAsJsonAsync("/api/Categories", new Category
        {
            Name = "Category For Stock Update",
            Tps = false,
            Tvq = false,
            IsActive = true
        });
        var category = await catResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var prodResponse = await client.PostAsJsonAsync("/api/Products", new Product
        {
            Name = "Product For Stock Update",
            PriceDefault = 10.0m,
            CategoryId = category.Id,
            IsActive = true
        });
        var product = await prodResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        // 3. Add to merchant inventory with initial Price = 10m, Stock = 5
        await client.PostAsJsonAsync("/api/MerchantProducts", new MerchantProduct
        {
            MerchantId = merchant.Id,
            ProductId = product.Id,
            Price = 10.0m,
            Stock = 5
        });

        // Act: Update price to 18.0m and add 10 to stock (should become 15)
        var modification = new ProductModification
        {
            ProductId = product.Id,
            NewPrice = 18.0m,
            QuantityToAdd = 10
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/MerchantProducts/{merchant.Id}", modification);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Verify updated price and stock
        var getResponse = await client.GetAsync($"/api/MerchantProducts/{merchant.Id}");
        var products = await getResponse.Content.ReadFromJsonAsync<MerchantProduct[]>();
        Assert.NotNull(products);

        var updatedItem = products.FirstOrDefault(p => p.ProductId == product.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(18.0m, updatedItem.Price);
        Assert.Equal(15, updatedItem.Stock);
    }

    [Fact]
    public async Task UpdateMerchantProduct_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var modification = new ProductModification
        {
            ProductId = -99999,
            NewPrice = 20.0m,
            QuantityToAdd = 5
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/MerchantProducts/-99999", modification);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddCategoryToMerchant_AndGetMerchantCategory_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant Category Linking Test",
            Phone = "555-6666"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // 2. Create category
        var catResponse = await client.PostAsJsonAsync("/api/Categories", new Category
        {
            Name = "Category For Merchant Link",
            Tps = true,
            Tvq = false,
            IsActive = true
        });
        var category = await catResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        // Act 1: Link category to merchant
        var linkResponse = await client.PostAsJsonAsync($"/api/MerchantCategory/{merchant.Id}", category.Id);

        // Assert 1: OK
        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);

        // Act 2: Link same category again (test idempotency)
        var secondLinkResponse = await client.PostAsJsonAsync($"/api/MerchantCategory/{merchant.Id}", category.Id);
        Assert.Equal(HttpStatusCode.OK, secondLinkResponse.StatusCode);

        // Act 3: GET /api/MerchantCategory/{merchantId}
        var getResponse = await client.GetAsync($"/api/MerchantCategory/{merchant.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task AddCategoryToMerchant_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act: Invalid merchant and invalid category
        var response = await client.PostAsJsonAsync("/api/MerchantCategory/-99999", -99999);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMerchant_WithoutSales_ShouldDeleteSuccessfully()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant To Delete Directly",
            Phone = "555-0199"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // Act: Delete merchant with no sales
        var deleteResponse = await client.DeleteAsync($"/api/Merchants/{merchant.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var result = await deleteResponse.Content.ReadFromJsonAsync<DeleteProductResponse>();
        Assert.NotNull(result);
        Assert.Contains("supprimé", result.Message);

        // Verify GET returns 404
        var getResponse = await client.GetAsync($"/api/Merchants/{merchant.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteMerchant_WithSales_ShouldDeactivateMerchant()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create merchant
        var merchantResponse = await client.PostAsJsonAsync("/api/Merchants", new Merchant
        {
            Adresse = "Merchant With Past Sales",
            Phone = "555-0188"
        });
        var merchant = await merchantResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(merchant);

        // 2. Create category and product
        var catResponse = await client.PostAsJsonAsync("/api/Categories", new Category
        {
            Name = "Cat For Merchant Sales",
            Tps = false,
            Tvq = false,
            IsActive = true
        });
        var category = await catResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var prodResponse = await client.PostAsJsonAsync("/api/Products", new Product
        {
            Name = "Prod For Merchant Sales",
            PriceDefault = 20.0m,
            CategoryId = category.Id,
            IsActive = true
        });
        var product = await prodResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        // 3. Link product to merchant
        await client.PostAsJsonAsync("/api/MerchantProducts", new MerchantProduct
        {
            MerchantId = merchant.Id,
            ProductId = product.Id,
            Price = 20.0m,
            Stock = 50
        });

        // 4. Create a sale for this merchant
        var saleRequest = new CreateSaleRequest
        {
            Date = DateTime.Now,
            Total = 20.0m,
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    ItemPrice = 20.0m,
                    TpsAmount = 0m,
                    TvqAmount = 0m
                }
            }
        };
        var saleResponse = await client.PostAsJsonAsync($"/api/Sales/{merchant.Id}", saleRequest);
        Assert.Equal(HttpStatusCode.OK, saleResponse.StatusCode);

        // Act: Delete merchant with sales
        var deleteResponse = await client.DeleteAsync($"/api/Merchants/{merchant.Id}");

        // Assert: 200 OK and message indicates deactivation
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var result = await deleteResponse.Content.ReadFromJsonAsync<DeleteProductResponse>();
        Assert.NotNull(result);
        Assert.Contains("désactivé", result.Message);

        // Verify merchant still exists but IsActive is false
        var getResponse = await client.GetAsync($"/api/Merchants/{merchant.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updatedMerchant = await getResponse.Content.ReadFromJsonAsync<Merchant>();
        Assert.NotNull(updatedMerchant);
        Assert.False(updatedMerchant.IsActive);
    }

    [Fact]
    public async Task DeleteMerchant_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/Merchants/-99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private class ProductModification
    {
        public int ProductId { get; set; }
        public decimal NewPrice { get; set; }
        public int QuantityToAdd { get; set; }
    }
}
