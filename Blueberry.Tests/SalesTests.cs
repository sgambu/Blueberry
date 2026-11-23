using Blueberry.Blazor;
using Blueberry.EntityModels;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blueberry.Blazor.Components.Pages;

namespace Blueberry.Tests
{
    public class SalesTests : BunitContext
    {
        private void AddTestServices()
        {
            var category = new Category
            {
                Id = 1,
                Name = "Boissons",
                Tps = true,
                Tvq = true
            };

            var merchantProduct = new MerchantProduct
            {
                ProductId = 1,
                Price = 2.50m,
                Stock = 10
            };

            var merchant = new Merchant
            {
                Id = 1,
                Adresse = "Marchand de test",
                Phone = "5145551234",
                Categories = new List<Category> { category },
                MerchantProducts = new List<MerchantProduct> { merchantProduct }
            };

            var merchantState = new SelectedMerchantState
            {
                SelectedMerchant = merchant
            };

            Services.AddSingleton(merchantState);

            var handler = new FakeHttpMessageHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            Services.AddSingleton(httpClient);
        }

        [Fact]
        public void Sales_ShouldRender()
        {
            AddTestServices();

            var cut = Render<Sales>();
        }

        [Fact]
        public void AddProduct_ShouldUpdateInvoiceAndTaxes()
        {
            AddTestServices();

            var cut = Render<Sales>();
            cut.Find("button").Click();

            // Sélectionner Coca-Cola
            cut.FindAll("button")
               .First(b => b.TextContent.Contains("Coca-Cola"))
               .Click();

            // Modifier la quantité
            cut.Find("input[type='number']").Change("2");

            // Cliquer sur OK
            cut.FindAll("button")
               .First(b => b.TextContent.Contains("OK"))
               .Click();

            // Vérifier le produit dans la facture
            Assert.Contains("Coca-Cola", cut.Markup);

            // Vérifier les montants
            Assert.Contains("5,00", cut.Markup);
            Assert.Contains("0,25", cut.Markup);
            Assert.Contains("0,49", cut.Markup);
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Coca-Cola",
                CategoryId = 1,
                PriceDefault = 2.50m
            }
        };

            var json = System.Text.Json.JsonSerializer.Serialize(products);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    json,
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            return Task.FromResult(response);
        }
    }
}