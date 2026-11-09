using System.Text.Json.Serialization;
using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Studium_Audit.Api.Extensions;
using Studium_Audit.Api.HostedServices;
using Studium_Audit.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=studium_audit;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AuditDbContext>(
    options => options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)),
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddProblemDetails();

builder.Services.CritterStackDefaults(options =>
{
    options.Development.GeneratedCodeMode = TypeLoadMode.Dynamic;
    options.Production.GeneratedCodeMode = TypeLoadMode.Static;
    options.Production.AssertAllPreGeneratedTypesExist = true;
});

builder.AddStudiumMessaging();
builder.AddStudiumAuthentication();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddStudiumCors(builder.Configuration);

var app = builder.Build();

app.UseCors(CorsExtensions.PolicyName);

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Studium-Audit" }));
app.MapHealthChecks("/health/ready");

return await app.RunJasperFxCommands(args);

public partial class Program { }
