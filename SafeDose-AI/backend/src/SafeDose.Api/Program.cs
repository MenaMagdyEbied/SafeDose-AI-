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
builder.Services.AddScoped<IPrescriptionRepository, SqlPrescriptionRepository>();

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


// Billing - repositories
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

// ─── Admin dashboard module ──────────────────────────────────────────────────
// Repositories
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminStatsRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminStatsRepository>();
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminAccountRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminAccountRepository>();
builder.Services.AddScoped<SafeDose.Application.Interfaces.Admin.IAdminPricingTierRepository,
                           SafeDose.Infrastructure.Repositories.Admin.SqlAdminPricingTierRepository>();

// Auth
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Auth.AdminLoginUseCase>();

// Dashboard use cases
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetDashboardKpisUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetAdminRevenueChartUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetGenderDistributionUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetTreatmentCardsUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetTeamBreakdownUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetFreeVsPaidUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Dashboard.GetRecentActivitiesUseCase>();

// PricingTiers admin use cases
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.GetAdminPricingTiersUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.UpdatePricingTierAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.AddFeatureUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.PricingTiers.RemoveFeatureUseCase>();

// Admin accounts management
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.ListAdminsUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.CreateAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.UpdateAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.DeleteAdminUseCase>();
builder.Services.AddScoped<SafeDose.Application.UseCases.Admin.Accounts.ToggleAdminStatusUseCase>();

// Dashboard pre-warm cache + background refresh (hourly)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SafeDose.Application.Caching.DashboardCache>();
builder.Services.AddHostedService<SafeDose.Application.BackgroundJobs.DashboardCacheRefreshService>();
// ─── End admin dashboard module ─────────────────────────────────────────────

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
builder.Services.AddScoped<SeedCriticalPairsUse