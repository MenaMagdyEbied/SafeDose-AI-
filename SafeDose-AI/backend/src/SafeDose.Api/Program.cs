// SafeDose AI - .NET 10 Web API
// Composition root — wires interfaces to implementations.

using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using SafeDose.Application.Interfaces;
using SafeDose.Application.UseCases;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Services;
using SafeDose.Infrastructure.ExternalServices;
using SafeDose.Infrastructure.Repositories;
using SafeDose.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// ─── Framework services ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── Database ───────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── CORS (for Doaa's Angular frontend) ─────────────────────────
const string CorsPolicy = "SafeDoseFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:3000",
                "https://safedose.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ─── Module 5: Drug Interaction Checker ────────────────────────

// Domain services (pure logic, singletons)
builder.Services.AddSingleton<AllergyCrossReactivityMatcher>();
builder.Services.AddSingleton<SeverityCalculator>();
builder.Services.AddSingleton<DuplicateDrugDetector>();
builder.Services.AddSingleton<CacheKeyHasher>();

// Repositories (Infrastructure)
builder.Services.AddScoped<IDrugRepository, SqlDrugRepository>();
builder.Services.AddScoped<IInteractionRepository, SqlInteractionRepository>();
builder.Services.AddScoped<ICriticalPairLookup, SqlCriticalPairLookup>();
builder.Services.AddScoped<IPatientRepository, SqlPatientRepository>();
builder.Services.AddScoped<IPatientMedicationProvider, SqlPatientMedicationProvider>();
builder.Services.AddScoped<IAuditLogService, SqlAuditLogService>();

// External services with Polly retry (3x exponential backoff per NFR-202)
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

// ─── Pipeline ───────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
