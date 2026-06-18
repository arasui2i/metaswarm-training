using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CRM.API.Authorization;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.Login;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

builder.Services.AddCors(cfg => {
    cfg.AddPolicy(
    "ReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("customers.view", policy =>
        policy.Requirements.Add(new PermissionRequirement("customers.view")));
    options.AddPolicy("customers.edit", policy =>
        policy.Requirements.Add(new PermissionRequirement("customers.edit")));
    options.AddPolicy("leads.view", policy =>
        policy.Requirements.Add(new PermissionRequirement("leads.view")));
    options.AddPolicy("leads.create", policy =>
        policy.Requirements.Add(new PermissionRequirement("leads.create")));
    options.AddPolicy("leads.edit", policy =>
        policy.Requirements.Add(new PermissionRequirement("leads.edit")));
    options.AddPolicy("leads.delete", policy =>
        policy.Requirements.Add(new PermissionRequirement("leads.delete")));
});
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                AutoReplenishment = true
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var adminRoleId = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1");
    var salesRoleId = Guid.Parse("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2");

    var seedUsers = new[]
    {
        new { Email = "admin@crm.com", Username = "admin", Password = "Admin@123", RoleId = adminRoleId },
        new { Email = "sales@crm.com", Username = "sales", Password = "Sales@123", RoleId = salesRoleId },
    };

    foreach (var seed in seedUsers)
    {
        if (!db.Users.Any(u => u.Email == seed.Email))
        {
            var user = new CRM.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = seed.Email,
                Username = seed.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            db.SaveChanges();

            db.UserRoles.Add(new CRM.Domain.Entities.UserRole { UserId = user.Id, RoleId = seed.RoleId });
            db.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
