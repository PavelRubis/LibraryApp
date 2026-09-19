using Application;
using Infrastructure;
using Infrastructure.Migrations;
using Serilog;
using WebAPI;
using WebAPI.Middleware;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureBootstrapLogger();
    builder.AddSerilogLogging();

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
    {
        app.Services.MigrateDatabase();
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "WebAPI terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
