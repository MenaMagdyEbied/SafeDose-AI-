using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Infrastructure.Seeders;

// Seeds the CriticalPair table with 30 well-known dangerous combinations.
// Stored by SCIENTIFIC NAME (not DrugId) so the seeder works
// regardless of which specific brands are in the catalog.
// Idempotent - running twice is safe (skips already-seeded pairs).
public class CriticalPairSeeder : ICriticalPairSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<CriticalPairSeeder> _logger;

    public CriticalPairSeeder(AppDbContext db, ILogger<CriticalPairSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Format: (scientificNameA, scientificNameB, reasonArabic, source)
    private static readonly (string A, string B, string ReasonAr, string Source)[] Pairs =
    {
        ("warfarin", "aspirin",          "زيادة خطر النزيف الحاد",           "DrugBank DB00682"),
        ("warfarin", "ibuprofen",        "زيادة خطر النزيف",                  "DrugBank DB00682"),
        ("warfarin", "clopidogrel",      "زيادة خطر النزيف",                  "DrugBank DB00682"),
        ("warfarin", "diclofenac",       "زيادة خطر النزيف",                  "DrugBank DB00682"),
        ("warfarin", "fluconazole",      "زيادة تأثير الوارفارين ونزيف",      "DrugBank DB00682"),
        ("perindopril", "spironolactone","فرط بوتاسيوم الدم",                 "FDA-Drug-Safety-2014"),
        ("enalapril",   "spironolactone","فرط بوتاسيوم الدم",                 "FDA-Drug-Safety-2014"),
        ("lisinopril",  "potassium",     "فرط بوتاسيوم الدم",                 "FDA-Drug-Safety-2014"),
        ("valsartan",   "spironolactone","فرط بوتاسيوم الدم",                 "FDA-Drug-Safety-2014"),
        ("fluoxetine",  "tramadol",       "متلازمة السيروتونين",              "FDA-Advisory-2016"),
        ("sertraline",  "tramadol",       "متلازمة السيروتونين",              "FDA-Advisory-2016"),
        ("escitalopram","tramadol",       "متلازمة السيروتونين",              "FDA-Advisory-2016"),
        ("paroxetine",  "sumatriptan",    "متلازمة السيروتونين",              "FDA-Advisory-2016"),
        ("sildenafil",  "nitroglycerin",  "هبوط حاد في ضغط الدم",             "DrugBank DB00203"),
        ("tadalafil",   "nitroglycerin",  "هبوط حاد في ضغط الدم",             "DrugBank DB00820"),
        ("vardenafil",  "isosorbide",     "هبوط حاد في ضغط الدم",             "DrugBank DB00862"),
        ("atorvastatin","erythromycin",   "خطر تكسر العضلات (Rhabdomyolysis)","DrugBank DB01076"),
        ("atorvastatin","clarithromycin", "خطر تكسر العضلات (Rhabdomyolysis)","DrugBank DB01076"),
        ("simvastatin", "itraconazole",   "خطر تكسر العضلات (Rhabdomyolysis)","DrugBank DB00641"),
        ("digoxin",     "verapamil",      "تسمم الديجوكسين",                  "DrugBank DB00390"),
        ("digoxin",     "amiodarone",     "تسمم الديجوكسين",                  "DrugBank DB00390"),
        ("lithium",     "hydrochlorothiazide","تسمم الليثيوم",                "DrugBank DB01356"),
        ("lithium",     "ibuprofen",      "تسمم الليثيوم",                    "DrugBank DB01356"),
        ("methotrexate","ibuprofen",      "تسمم الميثوتركسات",                "DrugBank DB00563"),
        ("methotrexate","trimethoprim",   "تثبيط نخاع العظم الحاد",            "DrugBank DB00563"),
        ("theophylline","ciprofloxacin",  "تسمم الثيوفيلين",                  "DrugBank DB00277"),
        ("tramadol",    "codeine",        "تثبيط تنفسي خطير",                 "FDA-Advisory-2017"),
        ("alprazolam",  "tramadol",       "تثبيط تنفسي خطير",                 "FDA-Advisory-2017"),
        ("metformin",   "iodine",         "خطر الحماض اللبني",                "ACR-Manual-2024"),
        ("furosemide",  "gentamicin",     "سمية أذنية حادة",                   "DrugBank DB00695"),
    };

    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _db.CriticalPairs
            .AsNoTracking()
            .Select(p => new { p.ScientificNameA, p.ScientificNameB })
            .ToListAsync(cancellationToken);

        var existingSet = new HashSet<string>(
            existing.Select(e => MakeKey(e.ScientificNameA, e.ScientificNameB)),
            StringComparer.OrdinalIgnoreCase);

        int inserted = 0;
        foreach (var (a, b, reason, source) in Pairs)
        {
            if (existingSet.Contains(MakeKey(a, b))) continue;

            _db.CriticalPairs.Add(new CriticalPair
            {
                ScientificNameA = a,
                ScientificNameB = b,
                DefaultLevel = InteractionLevel.Danger,
                ReasonArabic = reason,
                Source = source,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });
            inserted++;
        }

        if (inserted > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} new critical pairs", inserted);
        }
        else
        {
            _logger.LogInformation("Critical pairs already seeded - no changes");
        }

        return inserted;
    }

    private static string MakeKey(string? a, string? b)
    {
        var aa = (a ?? string.Empty).Trim().ToLowerInvariant();
        var bb = (b ?? string.Empty).Trim().ToLowerInvariant();
        // Order-independent - Warfarin+Aspirin = Aspirin+Warfarin
        return string.CompareOrdinal(aa, bb) < 0 ? $"{aa}|{bb}" : $"{bb}|{aa}";
    }
}
