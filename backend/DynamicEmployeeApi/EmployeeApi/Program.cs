using Dynamic.Employees.Application.Extensions;
using Dynamic.Json.AspNetCore;
using EmployeeApi.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterConfiguredEmployeeDatabase(builder.Configuration);
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
    await app.Services.ApplyEmployeeDatabaseMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
