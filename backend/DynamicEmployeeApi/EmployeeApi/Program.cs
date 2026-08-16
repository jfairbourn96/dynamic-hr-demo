using Dynamic.Employees.Application.Extensions;
using Dynamic.Employees.Data.SqlServer;
using Dynamic.Employees.Data.SqlServer.Extensions;
using Dynamic.Json.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.RegisterSqlServerEmployeeData(connectionString);
builder.Services.RegisterEmployeeApplicationServices();
builder.Services.AddDynamicJsonAspNetCore();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

WebApplication app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using IServiceScope scope = app.Services.CreateScope();
    SqlServerEmployeeDbContext dbContext = scope.ServiceProvider.GetRequiredService<SqlServerEmployeeDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
