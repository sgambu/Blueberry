using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using Blueberry.EntityModels.SqlServer.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Blueberry.Tests;

public class CategoryEndpointsTests
{
    [Fact]
    public async Task GetCategories_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<Category[]>();
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "GetById Test Category",
            Tps = true,
            Tvq = true,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdCategory = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(createdCategory);

        // Act
        var response = await client.GetAsync($"/api/Categories/{createdCategory.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        Assert.Equal(createdCategory.Id, category.Id);
        Assert.Equal("GetById Test Category", category.Name);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Categories/-99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetCategory_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "  Created Category Test  ",
            Tps = true,
            Tvq = false,
            IsActive = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Categories", newCategory);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Created Category Test", created.Name);
        Assert.True(created.Tps);
        Assert.False(created.Tvq);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task SetCategory_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var invalidCategory = new Category
        {
            Name = "   ",
            Tps = false,
            Tvq = false,
            IsActive = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Categories", invalidCategory);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategoryById_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "Category To Update By Id",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);

        created.Name = "Updated Category By Id";
        created.Tps = true;
        created.Tvq = true;

        // Act
        var response = await client.PutAsJsonAsync($"/api/Categories/{created.Id}", created);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Category By Id", updated.Name);
        Assert.True(updated.Tps);
        Assert.True(updated.Tvq);
    }

    [Fact]
    public async Task UpdateCategoryById_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var nonExistentCategory = new Category
        {
            Id = -99999,
            Name = "NonExistent Category",
            Tps = false,
            Tvq = false,
            IsActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Categories/-99999", nonExistentCategory);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategoryById_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "Valid Name Initially",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);

        created.Name = "";

        // Act
        var response = await client.PutAsJsonAsync($"/api/Categories/{created.Id}", created);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "Category To Update Via Body",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);

        created.Name = "Updated Via Body Name";

        // Act
        var response = await client.PutAsJsonAsync("/api/Categories", created);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Via Body Name", updated.Name);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var nonExistentCategory = new Category
        {
            Id = -99999,
            Name = "NonExistent",
            Tps = false,
            Tvq = false,
            IsActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Categories", nonExistentCategory);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "Valid Name Before Body Update",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);

        created.Name = "   ";

        // Act
        var response = await client.PutAsJsonAsync("/api/Categories", created);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/Categories/-99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithoutProducts_ShouldReturnNoContent()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        var newCategory = new Category
        {
            Name = "Category To Delete Directly",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(created);

        // Act
        var response = await client.DeleteAsync($"/api/Categories/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it was removed
        var getResponse = await client.GetAsync($"/api/Categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithActiveProducts_ShouldReturnOkWithWarningMessage()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create a category
        var newCategory = new Category
        {
            Name = "Category With Active Product",
            Tps = true,
            Tvq = false,
            IsActive = true
        };
        var createCatResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var category = await createCatResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        // 2. Create an active product in that category
        var product = new Product
        {
            Name = "Active Product in Cat",
            PriceDefault = 5.0m,
            CategoryId = category.Id,
            IsActive = true
        };
        var createProdResponse = await client.PostAsJsonAsync("/api/Products", product);
        Assert.Equal(HttpStatusCode.Created, createProdResponse.StatusCode);

        // Act: try deleting the category
        var response = await client.DeleteAsync($"/api/Categories/{category.Id}");

        // Assert: should return OK with message indicating deletion cannot proceed
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DeleteProductResponse>();
        Assert.NotNull(result);
        Assert.Contains("Impossible de supprimer", result.Message);
    }

    [Fact]
    public async Task DeleteCategory_WithOnlyInactiveProducts_ShouldDeactivateCategory()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Create a category
        var newCategory = new Category
        {
            Name = "Category With Inactive Product",
            Tps = false,
            Tvq = false,
            IsActive = true
        };
        var createCatResponse = await client.PostAsJsonAsync("/api/Categories", newCategory);
        var category = await createCatResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        // 2. Create an inactive product in that category
        var product = new Product
        {
            Name = "Inactive Product in Cat",
            PriceDefault = 5.0m,
            CategoryId = category.Id,
            IsActive = false
        };
        var createProdResponse = await client.PostAsJsonAsync("/api/Products", product);
        Assert.Equal(HttpStatusCode.Created, createProdResponse.StatusCode);

        // Act: delete the category
        var response = await client.DeleteAsync($"/api/Categories/{category.Id}");

        // Assert: should return OK with message indicating deactivation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DeleteProductResponse>();
        Assert.NotNull(result);
        Assert.Contains("désactivé", result.Message);

        // Verify the category is now inactive
        var getResponse = await client.GetAsync($"/api/Categories/{category.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updatedCat = await getResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(updatedCat);
        Assert.False(updatedCat.IsActive);
    }
}
