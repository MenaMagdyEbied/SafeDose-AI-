// SafeDose AI - .NET Web API
// Composition root — wires interfaces to implementations for ALL modules.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.Interfaces;
using SafeDose.Application.UseCases;
using SafeDose.Application.UseCases.Medication;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Services;
using SafeDose.Infrastructure.Auth;
using SafeDose.Infrastructure.ExternalServices;
using SafeDose.Infrastructure.Repositories;
using SafeDose.Infrastructure.Seeders;
using SafeDose.Shared.helper;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Framework services ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// ─── Database ───────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Module 1: Identity & Auth (Andrew) ─────────────────────────
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

builder.Services
    .AddIdentity<Account, IdentityRole>(options => options.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserGlobalServices, UserGlobalServices>();

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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
    };
});

// ─── Module 5: Drug Interaction Checker (Mina) ──────────────────

// Domain services (pure logic — singletons)
builder.Services.AddSingleton<AllergyCrossReactivityMatcher>();
builder.Services.AddSingleton<SeverityCalculator>();
builder.Services.AddSingleton<DuplicateDrugDetector>();
builder.Services.AddSingleton<CacheKeyHasher>();

// Repositories (Infrastructure)
builder.Services.AddScoped<IDrugRepository, SqlDrugRepository>();
builder.Services.AddScoped<IInteractionRepository, SqlInteractionRepository>();
builder.Services.AddScoped<ICriticalPairLookup, SqlCriticalPairLookup>();
builder.Services.AddScoped<IPatientRepository, SqlPatientRepository>();

// Module 4's full repository ALSO implements Module 5's narrow Provider.
// Register ONCE, expose under both interfaces.
builder.Services.AddScoped<SqlPatientMedicationRepository>();
builder.Services.AddScoped<IPatientMedicationRepository>(
    sp => sp.GetRequiredService<SqlPatientMedicationRepository>());
builder.Services.AddScoped<IPatientMedicationProvider>(
    sp => sp.GetRequiredService<SqlPatientMedicationRepository>());

builder.Services.AddScoped<IAuditLogService, SqlAuditLogService>();

// Langflow HTTP client with 3x exponential-backoff retry (NFR-202)
builder.Services
    .AddHttpClient<ILangflowClient, LangflowClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt =>
            TimeSpan.FromMilliseconds(200 * Math.Pow(3, attempt - 1))));

// Use cases
builder.Services.AddScoped<SearchDrugsUseCase>();
builder.Services.AddScoped<CheckDrugInteractionUseCase>();
builder.Services.AddScoped<CheckStandaloneInteractionUseCase>();
builder.Services.AddScoped<GetInteractionHistoryUseCase>();
builder.Services.AddScoped<GetInteractionCheckByIdUseCase>();
builder.Services.AddScoped<AcknowledgeWarningUseCase>();
builder.Services.AddScoped<DeleteInteractionCheckUseCase>();
builder.Services.AddScoped<GetPatientProfileSnapshotUseCase>();
builder.Services.AddScoped<SeedCriticalPairsUseCase>();

// Seeders + hosted services
builder.Services.AddScoped<CriticalPairSeeder>();
builder.Services.AddScoped<ICriticalPairSeeder>(sp => sp.GetRequiredService<CriticalPairSeeder>());
builder.Services.AddHostedService<CriticalPairSeederHostedService>();

// ─── Module 2: Patient Profile (Fady) ───────────────────────────
builder.Services.AddScoped<CreatePatientUseCase>();
builder.Services.AddScoped<UpdatePatientUseCase>();
builder.Services.AddScoped<GetMyPatientsUseCase>();
builder.Services.AddScoped<GetPatientByIdUseCase>();
builder.Services.AddScoped<DeactivatePatientUseCase>();

// ─── Module 4: Medication Management (Ahmed) ────────────────────
builder.Services.AddScoped<AddMedicationManuallyUseCase>();
builder.Services.AddScoped<AddMedicationsFromPrescriptionUseCase>();
builder.Services.AddScoped<UpdateMedicationUseCase>();
builder.Services.AddScoped<ChangeMedicationStatusUseCase>();
builder.Services.AddScoped<GetActiveMedicationsUseCase>();
builder.Services.AddScoped<GetMedicationHistoryUseCase>();
builder.Services.AddScoped<GetMedicationByIdUseCase>();

// ─── Swagger with JWT bearer ────────────────────────────────────
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nEnter 'Bearer' [space] and then your token.\r\nExample: \"Bearer 12345abcdef\""
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

// ─── CORS — single policy for Doaa's frontend ───────────────────
const string CorsPolicy = "allowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ─── Pipeline ───────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
