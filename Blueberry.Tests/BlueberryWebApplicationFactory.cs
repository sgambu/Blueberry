
extern alias WebApiApp; 

using Blueberry.DataContext.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueberry.Tests;

public class BlueberryWebApplicationFactory : WebApplicationFactory<WebApiApp::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var connectionString =
                "Data Source=localhost;Initial Catalog=Blueberry_Test;Integrated Security=true;TrustServerCertificate=true;";

            services.RemoveAll<DbContextOptions<BlueberryContext>>();

            services.AddDbContext<BlueberryContext>(options =>
                options.UseSqlServer(connectionString));
        });
    }
}
