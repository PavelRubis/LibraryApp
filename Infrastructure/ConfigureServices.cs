using Application.Dependencies.DataAccess;
using FluentMigrator.Runner;
using Infrastructure.Data;
using Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        services.AddScoped(_ => new DapperSession(connectionString));
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddFluentMigratorCore().ConfigureRunner(builder => builder.AddSqlServer().WithGlobalConnectionString(connectionString).ScanIn(typeof(CreateLibrarySchema).Assembly).For.Migrations().For.EmbeddedResources());

        return services;
    }
}
