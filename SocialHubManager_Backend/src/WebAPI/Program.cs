using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialHubManager_Backend.src.Application;
using SocialHubManager_Backend.src.Core.Interfaces;
using SocialHubManager_Backend.src.Infrastructure.Data;
using SocialHubManager_Backend.src.Infrastructure.Services;
using System.Text;
using TwoFactorService = SocialHubManager_Backend.src.Application.TwoFactorService;


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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<TwoFactorService>();

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

// ---------------------
// Authentication/Authorization
// ---------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey"))
    };
});
builder.Services.AddAuthorization();

 var app = builder.Build();

// ---------------------
// Middleware
// ---------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar CORS antes de Authorization
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

// Habilitar autenticación/authorization
app.UseAuthentication();
app.UseAuthorization();

// JWT Middleware personalizado
app.UseMiddleware<WebAPI.Middleware.JwtMiddleware>();

app.MapControllers();

app.Run();
