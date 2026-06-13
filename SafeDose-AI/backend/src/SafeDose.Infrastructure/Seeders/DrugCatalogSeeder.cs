using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using System.Globalization;

namespace SafeDose.Infrastructure.Seeders;

public class DrugCatalogSeeder : IDrugCatalogSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<DrugCatalogSeeder> _logger;
    private readonly string _csvPath;

    public DrugCatalogSeeder(
        AppDbContext db,
        IConfiguration configuration,
        ILogger<DrugCatalogSeeder> logger)
    {
        _db = db;
        _logger = logger;
        _csvPath = configuration["DrugCatalog:CsvPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "egyptian-drugs.csv");
    }

    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.DrugCatalogs.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("DrugCatalog already seeded - skipping");
            return 0;
        }

        if (!File.Exists(_csvPath))
        {
            _logger.LogWarning("DrugCatalog CSV not found at {Path}", _csvPath);
            return 0;
        }

        var inserted = 0;
        var skippedNoScientific = 0;
        using var reader = new StreamReader(_csvPath);
        await reader.ReadLineAsync(cancellationToken);

        var batch = new List<DrugCatalog>(500);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = SplitCsv(line);
            if (parts.Length < 7) continue;

            var commercialEn = parts[0].Trim();
            if (string.IsNullOrEmpty(commercialEn)) continue;
            if (commercialEn.Length > 255) commercialEn = commercialEn[..255];

            // No scientific name = cannot check interactions. Skip.
            var scientificName = parts[2].Trim();
            if (string.IsNullOrEmpty(scientificName))
            {
                skippedNoScientific++;
                continue;
            }

            decimal? price = null;
            if (decimal.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                price = p;

            batch.Add(new DrugCatalog
            {
                CommercialNameEn = commercialEn,
                CommercialNameAr = Truncate(parts[1].Trim(), 255),
                ScientificName = Truncate(scientificName, 500),
                Manufacturer = Truncate(parts[3].Trim(), 255),
                DrugClass = Truncate(parts[4].Trim(), 255),
                Route = Truncate(parts[5].Trim(), 80),
                PriceEgp = price,
            });

            if (batch.Count >= 500)
            {
                await _db.DrugCatalogs.AddRangeAsync(batch, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                inserted += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await _db.DrugCatalogs.AddRangeAsync(batch, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            inserted += batch.Count;
        }

        _logger.LogInformation(
            "Seeded {Count} drugs into DrugCatalog (skipped {Skipped} rows with no scientific name)",
            inserted, skippedNoScientific);
        return inserted;
    }

    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return null;
        return s.Length > max ? s[..max] : s;
    }

    private static string[] SplitCsv(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
