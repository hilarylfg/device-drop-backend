using device_drop_backend.Data;
using device_drop_backend.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()                   
            .AllowAnyHeader()                    
            .AllowCredentials();                
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await SeedData.InitializeAsync(app.Services);

app.UseCors("AllowFrontend");
app.MapControllers();

Console.WriteLine("Backend запущен успешно!");

app.Run();