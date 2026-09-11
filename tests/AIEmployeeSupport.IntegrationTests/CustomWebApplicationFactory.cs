using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using AIEmployeeSupport.Infrastructure.Persistence;

namespace AIEmployeeSupport.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<AIEmployeeSupport.Domain.Entities.User>, Microsoft.AspNetCore.Identity.PasswordHasher<AIEmployeeSupport.Domain.Entities.User>>();
        });
    }
}
