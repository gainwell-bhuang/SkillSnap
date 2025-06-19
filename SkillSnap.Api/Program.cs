using SkillSnap.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register Swagger services
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SkillSnapContext>(options =>
    options.UseSqlite("Data Source=skillsnap.db"));

builder.Services.AddControllers(); // Register controllers

var app = builder.Build();

// Auto-migrate database in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SkillSnapContext>();
        db.Database.Migrate();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers(); // Map controller endpoints

app.UseHttpsRedirection();


app.Run();
