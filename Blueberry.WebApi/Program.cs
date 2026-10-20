using Blueberry.EntityModels;
using Blueberry.DataContext.SqlServer;
using Microsoft.AspNetCore.HttpLogging;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<BlueberryContext>();
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

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

