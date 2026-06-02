// SafeDose AI - .NET 10 Web API
// Wire up dependency injection, controllers, and services here

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// TODO: Register repositories and external service clients
// Example:
// builder.Services.AddScoped<IPatientRepository, SqlPatientRepository>();
// builder.Services.AddScoped<ILangflowClient, LangflowClient>();
// builder.Services.AddScoped<CheckDrugInteractionUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
