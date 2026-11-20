using Blueberry.EntityModels;
using Blueberry.DataContext.SqlServer;
using Blueberry.EntityModels.SqlServer.DTOs;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    //builder.Services.AddDbContext<BlueberryContext>();

    var connectionString =
    builder.Configuration.GetConnectionString("Blueberry");

    builder.Services.AddDbContext<BlueberryContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddHttpLogging();
    builder.Services.Configure<HttpLoggingOptions>(options =>
    {
        options.LoggingFields = HttpLoggingFields.All;
        options.RequestBodyLogLimit = 4096;
        options.ResponseBodyLogLimit = 4096;
    });

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    app.UseHttpLogging();
    app.UseHttpsRedirection();

    app.MapProductEndpoints();
    app.MapSaleEndpoints();
    app.MapMerchantEndpoints();
    app.MapCategoryEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
public partial class Program { }


