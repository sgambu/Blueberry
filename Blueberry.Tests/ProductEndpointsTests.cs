using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

namespace Blueberry.Tests;

public class ProductEndpointsTests
{
    [Fact]
    public async Task GetProducts_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<Product[]>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }
    [Fact]
    public async Task SetProducts_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        Product newProduct = new Product
        {
            Name = "Test Product",
            PriceDefault = 10.0m,
            CategoryId = 1,
            IsActive = true
        };
        var response = await client.PostAsJsonAsync("/api/Products", newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Créer un produit
        Product newProduct = new Product
        {
            Name = "Product to Update",
            PriceDefault = 10.0m,
            CategoryId = 1,
            IsActive = true
        };

        var createResponse = await client.PostAsJsonAsync("/api/Products", newProduct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Récupérer le produit créé
        Product? createdProduct =
            await createResponse.Content.ReadFromJsonAsync<Product>();

        Assert.NotNull(createdProduct);

        createdProduct.Name = "Updated Product Name"; // Modifier le nom
        createdProduct.PriceDefault = 20.0m; // Modifier le prix par défaut

        var response = await client.PutAsJsonAsync("/api/Products", createdProduct);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Vérifier que les modifications ont été enregistrées
        var getResponse = await client.GetAsync("/api/Products");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var products = await getResponse.Content.ReadFromJsonAsync<Product[]>();

        Assert.NotNull(products);

        var updatedProduct = products.FirstOrDefault(
            p => p.Id == createdProduct.Id);

        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product Name", updatedProduct.Name);
        Assert.Equal(20.0m, updatedProduct.PriceDefault);
    }
    [Fact]
    public async Task DeleteProduct_ShouldReturnSuccess()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Créer un produit
        Product newProduct = new Product
        {
            Name = "Product to Delete",
            PriceDefault = 10.0m,
            CategoryId = 1,
            IsActive = true
        };
        var createResponse = await client.PostAsJsonAsync("/api/Products", newProduct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Récupérer le produit créé
        Product? createdProduct = await createResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);

        var response = await client.DeleteAsync($"/api/Products/{createdProduct.Id}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound()
    {
        // Arrange
        var factory = new BlueberryWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/Products",
            new Product
            {
                Id = -1,
                Name = "Unknown Product",
                PriceDefault = 10.0m,
                CategoryId = 1,
                IsActive = true
            });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

}


