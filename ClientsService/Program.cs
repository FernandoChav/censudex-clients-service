using System.Reflection;
using ClientsService.Data;
using ClientsService.Services;
using ClientsService.Validators; // Importa el de validators
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Clients;
using ClientsService.Interceptors;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Add PostgreSQL DbContext
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ClientsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Add gRPC

        builder.Services.AddSingleton<ValidationInterceptor>();
        builder.Services.AddGrpc(options =>
        {
            // Le decimos a gRPC que use nuestro interceptor globalmente
            options.Interceptors.Add<ValidationInterceptor>();
        });
        // 3. Add AutoMapper
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // 4. Add FluentValidation
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddScoped<IValidator<CreateClientRequest>, CreateClientValidator>();
        builder.Services.AddScoped<IValidator<UpdateClientRequest>, UpdateClientValidator>();

        var app = builder.Build();

        // 5. Map gRPC Service
        app.MapGrpcService<ClientsGrpcService>();

        app.MapGet("/", () => "Clients Service is running. Ready for gRPC.");

        // 6. Run the Data Seeder
        DataSeeder.SeedDatabase(app);

        app.Run();
    }
}