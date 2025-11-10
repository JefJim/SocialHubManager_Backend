using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Application;
using SocialHubManager_Backend.src.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------------------
// Configuración de DbContext
// ---------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------
// Registro de servicios
// ---------------------
builder.Services.AddScoped<AuthService>();

// ---------------------
// CORS para Angular
// ---------------------
var MyAllowSpecificOrigins = "AllowAngularOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ---------------------
// Swagger/OpenAPI
// ---------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------
// Controllers
// ---------------------
builder.Services.AddControllers();

var app = builder.Build();

// ---------------------
// Middleware
// ---------------------
app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
