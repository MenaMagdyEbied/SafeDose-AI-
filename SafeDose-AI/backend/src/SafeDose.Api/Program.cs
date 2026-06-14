using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.Interfaces;
using SafeDose.Application.UseCases;
using SafeDose.Application.UseCases.Medication;
using SafeDose.Application.UserProfile.RepositoryInterface;
using SafeDose.Application.UserProfile.ServicesInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Services;
using SafeDose.Infrastructure.Auth;
using SafeDose.Infrastructure.ExternalServices;
using SafeDose.Infrastructure.Repositories;
using SafeDose.Infrastructure.Seeders;
using SafeDose.Infrastructure.UserProfile.RepositoryImplementation;
using SafeDose.Infrastructure.UserProfile.ServicesImplementation;
using SafeDose.Shared.helper;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(3);
});

builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

builder.Services
    .AddIdentity<Account, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserGlobalServices, UserGlobalServices>();
builder.Services.AddScoped<IEmailSender, EmailSernder>();
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
    // Don't let the middleware remap "sub" to NameIdentifier - keeps user.Id distinct from userName
    options.MapInboundClaims = false;
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

// Domain services
builder.Services.AddSingleton<AllergyCrossReactivityMatcher>();
builder.Services.AddSingleton<SeverityCalculator>();
builder.Services.AddSingleton<DuplicateDrugDetector>();
builder.Services.AddSingleton<CacheKeyHasher>();

// Repositories
builder.Services.AddScoped<IDrugRepository, SqlDrugRepository>();
builder.Services.AddScoped<IInteractionRepository, SqlInteractionRepository>();
builder.Services.AddScoped<ICriticalPairLookup, SqlCriticalPairLookup>();
builder.Services.AddScoped<IPatientRepository, SqlPatientRepository>();

// One class, two interfaces (full repo + read-only provider)
builder.Services.AddScoped<SqlPatientMedicationRepository>();
builder.Services.AddScoped<IPatientMedicationRepository>(
    sp => sp.GetRequiredService<SqlPatientMedicationRepository>());
builder.Services.AddScoped<IPatientMedicationProvider>(
    sp => sp.GetRequiredService<SqlPatientMedicationRepository>());

builder.Services.AddScoped<IAuditLogService, SqlAuditLogService>();

// Langflow client with retry
builder.Services
    .AddHttpClient<ILangflowClient, LangflowClient>(client =>
    {
        // Langflow LLM calls can take a while on cold start - 30s wasn't enough
        client.Timeout = TimeSpan.FromSeconds(120);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt =>
            TimeSpan.FromMilliseconds(200 * Math.Pow(3, attempt - 1))));

// Drug Interaction use cases
builder.Services.AddScoped<SearchDrugsUseCase>();
builder.Services.AddScoped<CheckDrugInteractionUseCase>();
builder.Services.AddScoped<CheckStandaloneInteractionUseCase>();
builder.Services.AddScoped<CheckCatalogInteractionsUseCase>();
builder.Services.AddScoped<GetInteractionHistoryUseCase>();
builder.Services.AddScoped<GetInteractionCheckByIdUseCase>();
builder.Services.AddScoped<AcknowledgeWarningUseCase>();
builder.Services.AddScoped<DeleteInteractionCheckUseCase>();
builder.Services.AddScoped<GetPatientProfileSnapshotUseCase>();
builder.Services.AddScoped<SeedCriticalPairsUseCase>();

// CriticalPair seeder runs on startup
builder.Services.AddScoped<CriticalPairSeeder>();
builder.Services.AddScoped<ICriticalPairSeeder>(sp => sp.GetRequiredService<CriticalPairSeeder>());
builder.Services.AddHostedService<CriticalPairSeederHostedService>();

// DrugCatalog seeder runs on startup (reads CSV)
builder.Services.AddScoped<DrugCatalogSeeder>();
builder.Services.AddScoped<IDrugCatalogSeeder>(sp => sp.GetRequiredService<DrugCatalogSeeder>());
builder.Services.AddHostedService<DrugCatalogSeederHostedService>();

// Patient use cases
builder.Services.AddScoped<CreatePatientUseCase>();
builder.Services.AddScoped<UpdatePatientUseCase>();
builder.Services.AddScoped<GetMyPatientsUseCase>();
builder.Services.AddScoped<GetPatientByIdUseCase>();
builder.Services.AddScoped<DeactivatePatientUseCase>();

// Medication use cases
builder.Services.AddScoped<AddMedicationManuallyUseCase>();
builder.Services.AddScoped<AddMedicationsFromPrescriptionUseCase>();
builder.Services.AddScoped<UpdateMedicationUseCase>();
builder.Services.AddScoped<ChangeMedicationStatusUseCase>();
builder.Services.AddScoped<GetActiveMedicationsUseCase>();
builder.Services.AddScoped<GetMedicationHistoryUseCase>();
builder.Services.AddScoped<GetMedicationByIdUseCase>();

// Swagger with JWT bearer
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token.\r\nExample: \"Bearer 12345abcdef\""

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

const string CorsPolicy = "allowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

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