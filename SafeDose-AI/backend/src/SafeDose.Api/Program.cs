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
using SafeDose.Application.UseCases.Billing;
using SafeDose.Application.UseCases.Medication;
using SafeDose.Infrastructure.ExternalServices;
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
builder.Services.AddScoped<IPrescriptionRepository, SqlPrescriptionRepository>();
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
        client.Timeout = TimeSpan.FromSeconds(120);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt =>
            TimeSpan.FromMilliseconds(200 * Math.Pow(3, attempt - 1))));

// Billing - repositories
builder.Services.AddScoped<IFreeTierUsageRepository, SqlFreeTierUsageRepository>();
builder.Services.AddScoped<IPricingTierRepository, SqlPricingTierRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SqlSubscriptionRepository>();
builder.Services.AddScoped<IPaymentRepository, SqlPaymentRepository>();

// Billing - use cases
builder.Services.AddScoped<GetPricingTiersUseCase>();
builder.Services.AddScoped<GetMySubscriptionUseCase>();
builder.Services.AddScoped<InitiateCheckoutUseCase>();
builder.Services.AddScoped<ProcessPaymobWebhookUseCase>();
builder.Services.AddScoped<CancelSubscriptionUseCase>();
builder.Services.AddScoped<GetPaymentStatusUseCase>();

// Paymob HTTP client + options
builder.Services.Configure<PaymobOptions>(builder.Configuration.GetSection("Paymob"));
builder.Services
    .AddHttpClient<IPaymobClient, PaymobClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(20);
    });

// PricingTier seeder runs on startup
builder.Services.AddScoped<PricingTierSeeder>();
builder.Services.AddScoped<IPricingTierSeeder>(sp => sp.GetRequiredService<PricingTierSeeder>());
builder.Services.AddHostedService<PricingTierSeederHostedService>();

// Langflow prescription client
builder.Services
    .AddHttpClient<ILangflowPrescriptionClient, LangflowPrescriptionClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(500);
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
builder.Services.AddScoped<ParsePrescriptionUseCase>();
builder.Services.AddScoped<SavePrescriptionUseCase>();
builder.Services.AddScoped<GetPatientPrescriptionsUseCase>();
builder.Services.AddScoped<GetPrescriptionDetailsUseCase>();
builder.Services.AddScoped<DeletePrescriptionUseCase>();

// Patient use cases (PatientsController) — without these, /api/patients/* returns
// 500 "Unable to resolve service for type 'CreatePatientUseCase'".
builder.Services.AddScoped<CreatePatientUseCase>();
builder.Services.AddScoped<UpdatePatientUseCase>();
builder.Services.AddScoped<GetMyPatientsUseCase>();
builder.Services.AddScoped<GetPatientByIdUseCase>();
builder.Services.AddScoped<DeactivatePatientUseCase>();

// Medication use cases (MedicationsController) — already using SafeDose.Application.UseCases.Medication
builder.Services.AddScoped<AddMedicationManuallyUseCase>();
builder.Services.AddScoped<AddMedicationsFromPrescriptionUseCase>();
builder.Services.AddScoped<UpdateMedicationUseCase>();
builder.Services.AddScoped<ChangeMedicationStatusUseCase>();
builder.Services.AddScoped<GetActiveMedicationsUseCase>();
builder.Services.AddScoped<GetMedicationHistoryUseCase>();
builder.Services.AddScoped<GetMedicationByIdUseCase>();

// MedicalCard use cases (MedicalCardController) — public + private card, QR, PDF.
builder.Services.AddScoped<GetPublicMedicalCardUseCase>();
builder.Services.AddScoped<GetPrivateMedicalCardUseCase>();
builder.Services.AddScoped<GenerateQrCodeUseCase>();
builder.Services.AddScoped<GenerateMedicalCardPdfUseCase>();

// Admin dashboard cache — shared between background warmer and the controller.
// Singleton so both sides read/write the same IMemoryCache-backed view.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SafeDose.Application.Caching.DashboardCache>();
builder.Services.AddHostedService<SafeDose.Application.BackgroundJobs.DashboardCacheRefreshService>();

// CriticalPair seeder runs on startup
builder.Services.AddScoped<CriticalPairSeeder>();
builder.Services.AddScoped<ICriticalPairSeeder>(sp => sp.GetRequiredService<CriticalPairSeeder>());
builder.Services.AddHostedService<CriticalPairSeederHostedService>();

// DrugCatalog seeder runs on startup (reads CSV)
builder.Services.AddScoped<DrugCatalogSeeder>();
builder.Services.AddScoped<IDrugCatalogSeeder>(sp => sp.GetRequiredService<DrugCatalogSeeder>());
builder.Services.AddHostedService<DrugCatalogSeederHostedService>();

// ─── Admin dashboard module ──────────────────────────────────────────────────
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminStatsRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminStatsRepository>();
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminAccountRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminAccountRepository>();
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminPricingTierRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminPricingTierRepository>();

builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Auth.AdminLoginUseCase>();

builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetDashboardKpisUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetAdminRevenueChartUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetGenderDistributionUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetTreatmentCardsUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetTeamBreakdownUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetFreeVsPaidUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetRecentActivitiesUseCase>();

builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.GetAdminPricingTiersUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.UpdatePricingTierAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.AddFeatureUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.RemoveFeatureUseCase>();

builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.ListAdminsUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.CreateAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.UpdateAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.DeleteAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.ToggleAdminStatusUseCase>();

// Chatbot
builder.Services.AddScoped<SafeDose.Application.UseCases.Chatbot.GetChatbotPatientContextUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Chatbot.ProcessChatMessageUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Chatbot.ProcessPublicChatMessageUseCase>();
builder.Services
    .AddHttpClient<SafeDose.Application.Interfaces.IChatLlmClient, SafeDose.Infrastructure.ExternalServices.FireworksChatLlmClient>(c =>
    {
        c.Timeout = TimeSpan.FromSeconds(60);
    });

// Dashboard pre-warm cache + background refresh
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SafeDose.Application.Caching.DashboardCache>();
builder.Services.AddHostedService<SafeDose.Application.BackgroundJobs.DashboardCacheRefreshService>();
// ─── End admin dashboard module ─────────────────────────────────────────────

// CORS
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SafeDose API", Version = "v1" });
    // ApiKey scheme so the user types "Bearer <token>" themselves — matches the
    // way the team has been entering tokens since Ahmed set this up.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer",
        In          = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer eyJhbGciOi...'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();