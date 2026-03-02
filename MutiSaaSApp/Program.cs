using Application.Constants;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;
using FluentValidation;
using Infastructure.Data;
using Infastructure.Repositories;
using Infastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MutiSaaSApp.Authorization;
using MutiSaaSApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrgUserRepository, OrgUserRepository>();
builder.Services.AddScoped<IInviteTokenRepository, InviteTokenRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Register Caching Service
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.Configuration = redisConnection;
});
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddSwaggerGen();

// Register JWT Token Service
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-secret-key-change-in-production";
var jwtExpiryMinutes = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "60");
builder.Services.AddScoped<IJwtTokenService>(sp => new JwtTokenService(jwtSecret, jwtExpiryMinutes));

// Register Authorization Handlers
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.Requirements.Add(new AdminRoleRequirement()))
    .AddPolicy(AuthorizationPolicies.MemberOrAdmin, policy =>
        policy.Requirements.Add(new MemberOrAdminRequirement()));

builder.Services.AddScoped<IAuthorizationHandler, AdminRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MemberOrAdminHandler>();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining(typeof(RegisterOrganizationValidator));

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = null; // We're not using an external authority
        options.RequireHttpsMetadata = false; // For development only
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "TeamFlow",
            ValidateAudience = true,
            ValidAudience = "TeamFlow",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Register Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Register Organization Membership Middleware
app.UseMiddleware<OrganizationMembershipMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.Run();
