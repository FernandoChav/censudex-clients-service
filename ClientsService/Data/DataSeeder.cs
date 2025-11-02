using ClientsService.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientsService.Data;

public static class DataSeeder
{
    public static void SeedDatabase(IHost app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
            
            // Apply pending migrations
            context.Database.Migrate();

            // Add seed data if table is empty
            if (!context.Clients.IgnoreQueryFilters().Any())
            {
                var admin = new Client
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "Censudex",
                    Email = "admin@censudex.cl",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Pwd: Admin123!
                    BirthDate = new DateOnly(1990, 1, 1),
                    PhoneNumber = "+56912345678",
                    Address = "123 Main St",
                    Status = "active",
                    Role = "admin",
                    RegistrationDate = DateTime.UtcNow
                };
                
                context.Clients.Add(admin);
                context.SaveChanges();
            }
        }
    }
}