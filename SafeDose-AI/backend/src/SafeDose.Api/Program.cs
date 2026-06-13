// SafeDose AI - .NET 10 Web API
// Wire up dependency injection, controllers, and services here

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.UserProfile.RepositoryInterface;
using SafeDose.Application.UserProfile.ServicesInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Infrastructure.Auth;
using SafeDose.Infrastructure.UserProfile.RepositoryImplementation;
using SafeDose.Infrastructure.UserProfile.ServicesImplementation;
using SafeDose.Shared.helper;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
builder.Services.AddHttpContextAccessor();



builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(3);
});


builder.Services.AddIdentity<Account, IdentityRole>(options => { 
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);

    //email confiramtion shortly
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// TODO: Register repositories and external service clients
// Example:
// builder.Services.AddScoped<IPatientRepository, SqlPatientRepository>();
// builder.Services.AddScoped<ILangflowClient, LangflowClient>();
// builder.Services.AddScoped<CheckDrugInteractionUseCase>();

builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IUserGlobalServices, UserGlobalServices>();
builder.Services.AddScoped<IEmailSender,EmailSernder>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserProfileServices, UserProfileServices>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = false;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });


builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                           new OpenApiSecurityScheme
                             {
                                  Reference = new OpenApiReference
                                     {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                     },
                                  Name = "Bearer",
                                  In = ParameterLocation.Header
                              },
                          new List<string>()
                     }
                 });

});





builder.Services.AddCors(options =>
{
    options.AddPolicy("allowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("allowAll");
app.MapControllers();

app.Run();
