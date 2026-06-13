# Module 5 — Drug Interaction Checker — Build Plan

> **Owner:** Mina Magdy Ebied
> **Status:** Starting build
> **Target completion:** 4 weeks
> **Last updated:** 2026-06-07

---

## How to use this document

This is the BUILD PLAN — the practical, week-by-week, task-by-task roadmap to ship Module 5.

It complements:
- `module-5-requirements.md` (the WHAT)
- `module-distribution-guide.md` (the team structure)

This document is the HOW + WHEN. Every task has:
- Concrete deliverable
- Files to create
- Definition of Done
- Estimated time
- Dependencies

Update the [x] checkboxes as you finish tasks.

---

# PHASE 0 — PRE-BUILD CHECKLIST (Day 1, 2 hours)

Before writing any code, verify these are in place.

## 0.1 Repository setup

- [ ] Pull latest from GitHub (Mina's updated SafeDose-AI repo)
- [ ] Verify backend folder structure matches `module-distribution-guide.md` Part 2
- [ ] Confirm Auth module (Andrew's) compiles and runs locally
- [ ] Run Andrew's auth flow end-to-end with Postman (Login → OTP → JWT)

## 0.2 Database baseline

- [ ] EF migrations from Andrew's work applied to local SQL Server
- [ ] `Account`, `Role`, `OTPRequest`, `ConsentRecord` tables exist
- [ ] Seed at least 1 test Account + 1 test Patient (you can use a SQL script)

## 0.3 External services accessible

- [ ] Langflow running locally on `localhost:7860`
- [ ] Existing Drug Search + OCR flows working (test with old prescription)
- [ ] Pinecone `safedose-drugs` index responsive (test via Pinecone console)
- [ ] Vertex AI / Fireworks API credentials configured in Langflow
- [ ] HuggingFace `bge-m3` accessible (or fallback configured)

## 0.4 Coordination notes

- [ ] Sent message to Fady: "I need a Patient profile read API by end of Week 1. I'll stub it for now and swap in your real one when ready."
- [ ] Sent message to Ahmed: "I need a list-active-medications endpoint by Week 2. Same plan — stub then swap."
- [ ] Sent message to Doaa: "Frontend wiring for Module 5 starts Week 4. Here's the API contract."

---

# PHASE 1 — FOUNDATION (Week 1)

## Goal of this phase

Build the deterministic, non-AI safety layer FIRST. This gives you a working "Critical Pair" check that runs without any LLM dependency. Even if Langflow crashes, this safety net catches the most dangerous interactions.

**Why first:** Risk reduction. LLM-based checks are nice-to-have. Critical-pair checks are non-negotiable for patient safety. Build the must-have first.

## 1.1 Domain entities (Day 1)

**Task:** Create the entity classes.

**Files to create:**
```
SafeDose.Domain/Entities/Interaction/
├── InteractionCheck.cs
└── CriticalPair.cs
```

**Code structure:**

```csharp
// InteractionCheck.cs
public class InteractionCheck : AuditableEntity
{
    public Guid InteractionCheckId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? NewDrugId { get; private set; }
    public string NewDrugName { get; private set; }
    public int Level { get; private set; }                      // 1, 2, 3
    public string LabelArabic { get; private set; }
    public string ExplanationArabic { get; private set; }
    public string RecommendedActionArabic { get; private set; }
    public string ConflictingDrugsJson { get; private set; }
    public string ConflictingConditionsJson { get; private set; }
    public string SourcesJson { get; private set; }
    public string ModelVersion { get; private set; }
    public string PineconeIndexVersion { get; private set; }
    public Guid ConsentRecordId { get; private set; }
    public DateTime CheckedAt { get; private set; }
    public bool IsBackgroundRecheck { get; private set; }
    public Guid? PreviousCheckId { get; private set; }
    
    private InteractionCheck() { }
    
    public static InteractionCheck Create(
        Guid patientId,
        Guid? newDrugId,
        string newDrugName,
        int level,
        /* etc */)
    {
        // validation
        if (level < 1 || level > 3)
            throw new ArgumentException("Level must be 1, 2, or 3");
        
        return new InteractionCheck { /* assign */ };
    }
}

// CriticalPair.cs
public class CriticalPair
{
    public int CriticalPairId { get; private set; }
    public Guid DrugIdA { get; private set; }
    public Guid DrugIdB { get; private set; }
    public int DefaultLevel { get; private set; } = 3;
    public string ReasonArabic { get; private set; }
    public string Source { get; private set; }
    
    // factory + validation
}
```

**Definition of Done:**
- [ ] Both classes compile
- [ ] Factory methods validate inputs (throw on invalid)
- [ ] Properties are private setters (immutability)
- [ ] No external dependencies (no EF, no HTTP)

**Time:** 2 hours

---

## 1.2 EF Configurations (Day 1)

**Task:** Tell EF Core how to map these entities to SQL.

**Files to create:**
```
SafeDose.Infrastructure/Persistence/Configurations/Interaction/
├── InteractionCheckConfiguration.cs
└── CriticalPairConfiguration.cs
```

**Code structure:**

```csharp
public class InteractionCheckConfiguration : IEntityTypeConfiguration<InteractionCheck>
{
    public void Configure(EntityTypeBuilder<InteractionCheck> builder)
    {
        builder.ToTable("InteractionChecks");
        builder.HasKey(x => x.InteractionCheckId);
        builder.Property(x => x.NewDrugName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LabelArabic).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExplanationArabic).HasMaxLength(2000);
        builder.Property(x => x.ConflictingDrugsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SourcesJson).HasColumnType("nvarchar(max)");
        
        // Indexes for common queries
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => new { x.PatientId, x.CheckedAt });
        builder.HasIndex(x => new { x.PatientId, x.NewDrugId, x.CheckedAt });  // for caching
        
        // FK to Patient (when Fady's module is ready)
        builder.HasOne<Patient>().WithMany().HasForeignKey(x => x.PatientId);
    }
}
```

**Definition of Done:**
- [ ] Both configurations compile
- [ ] Indexes match expected query patterns
- [ ] String columns have explicit max lengths
- [ ] JSON columns use `nvarchar(max)`

**Time:** 1 hour

---

## 1.3 Add to SafeDoseDbContext (Day 1)

**Task:** Register the new entities.

**File to modify:**
`SafeDose.Infrastructure/Persistence/DbContext/SafeDoseDbContext.cs`

**Add:**
```csharp
public DbSet<InteractionCheck> InteractionChecks { get; set; }
public DbSet<CriticalPair> CriticalPairs { get; set; }
```

**Definition of Done:**
- [ ] DbContext compiles
- [ ] Run `dotnet ef migrations add Module5Initial`
- [ ] Run `dotnet ef database update`
- [ ] Verify tables exist in SQL Server

**Time:** 30 minutes

**Coordination:** This is the FIRST shared-file edit. Open a PR, get Mina's review (since you ARE Mina, just commit directly).

---

## 1.4 Seed CriticalPair table (Day 2)

**Task:** Insert the 30+ known dangerous drug combinations.

**File to create:**
```
SafeDose.Infrastructure/Persistence/Seeders/
└── CriticalPairSeeder.cs
```

**Approach:**

```csharp
public class CriticalPairSeeder
{
    private readonly SafeDoseDbContext _db;
    private readonly IDrugRepository _drugRepo;
    
    public async Task SeedAsync()
    {
        if (await _db.CriticalPairs.AnyAsync()) return;  // already seeded
        
        var pairs = new[]
        {
            ("Warfarin", "Aspirin", "زيادة خطر النزيف", "DrugBank DB00682"),
            ("Warfarin", "Ibuprofen", "زيادة خطر النزيف", "DrugBank DB00682"),
            ("Warfarin", "Clopidogrel", "زيادة خطر النزيف", "..."),
            // ... all 30 from Appendix A of requirements doc
        };
        
        foreach (var (drugA, drugB, reason, source) in pairs)
        {
            var drugAId = await _drugRepo.FindByScientificNameAsync(drugA);
            var drugBId = await _drugRepo.FindByScientificNameAsync(drugB);
            if (drugAId == null || drugBId == null) continue;  // skip if not in DB
            
            await _db.CriticalPairs.AddAsync(CriticalPair.Create(
                drugAId.Value, drugBId.Value, 3, reason, source));
        }
        
        await _db.SaveChangesAsync();
    }
}
```

**Definition of Done:**
- [ ] At least 25 critical pairs seeded (some may not have matching drugs in your Pinecone DB)
- [ ] Seeder runs on application startup if table empty
- [ ] Log how many pairs successfully seeded
- [ ] Test: query `SELECT * FROM CriticalPairs` returns rows

**Time:** 3 hours

**Note:** If a drug name doesn't match in your DB, log it as a warning and continue. The seeder is best-effort.

---

## 1.5 Repository interfaces (Day 2)

**Task:** Define the contracts.

**Files to create:**
```
SafeDose.Application/Modules/Interaction/Interfaces/
├── IInteractionRepository.cs
├── ICriticalPairLookup.cs
├── ILangflowClient.cs
├── IPineconeClient.cs
└── IPatientContextProvider.cs   // abstraction for cross-module data
```

**Sample:**

```csharp
public interface IInteractionRepository
{
    Task<InteractionCheck?> GetByIdAsync(Guid id);
    Task<List<InteractionCheck>> GetHistoryForPatientAsync(Guid patientId, int limit, int offset);
    Task<InteractionCheck?> GetCachedResultAsync(Guid patientId, Guid drugId, TimeSpan maxAge);
    Task AddAsync(InteractionCheck check);
    Task<int> CountByPatientAsync(Guid patientId);
}

public interface ICriticalPairLookup
{
    Task<CriticalPair?> FindPairAsync(Guid drugIdA, Guid drugIdB);
    Task<List<CriticalPair>> FindAllPairsForDrugAsync(Guid drugId);
}

public interface IPatientContextProvider
{
    Task<PatientContextSnapshot> GetSnapshotAsync(Guid patientId);
}
```

`IPatientContextProvider` is your DECOUPLING layer. You don't directly depend on Fady's repository. You depend on this abstraction. When Fady ships, you swap implementations.

**Definition of Done:**
- [ ] All 5 interfaces compile
- [ ] Method names are business-meaningful (not generic `Find()`)
- [ ] Return types use `Task<T>` for async

**Time:** 2 hours

---

## 1.6 Repository implementations (Day 3)

**Files to create:**
```
SafeDose.Infrastructure/Persistence/Repositories/Interaction/
├── InteractionRepository.cs
└── CriticalPairLookup.cs
```

```csharp
public class InteractionRepository : IInteractionRepository
{
    private readonly SafeDoseDbContext _db;
    public InteractionRepository(SafeDoseDbContext db) => _db = db;
    
    public async Task<InteractionCheck?> GetByIdAsync(Guid id) =>
        await _db.InteractionChecks.FindAsync(id);
    
    public async Task<List<InteractionCheck>> GetHistoryForPatientAsync(
        Guid patientId, int limit, int offset) =>
        await _db.InteractionChecks
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CheckedAt)
            .Skip(offset).Take(limit)
            .ToListAsync();
    
    public async Task<InteractionCheck?> GetCachedResultAsync(
        Guid patientId, Guid drugId, TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        return await _db.InteractionChecks
            .Where(c => c.PatientId == patientId 
                     && c.NewDrugId == drugId 
                     && c.CheckedAt > cutoff)
            .OrderByDescending(c => c.CheckedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task AddAsync(InteractionCheck check)
    {
        await _db.InteractionChecks.AddAsync(check);
        await _db.SaveChangesAsync();
    }
    
    public async Task<int> CountByPatientAsync(Guid patientId) =>
        await _db.InteractionChecks.CountAsync(c => c.PatientId == patientId);
}
```

**Definition of Done:**
- [ ] Both compile
- [ ] All interface methods implemented
- [ ] EF queries use `async/await`
- [ ] Cache lookup uses index (verify execution plan)

**Time:** 3 hours

---

## 1.7 Stub Patient Context Provider (Day 3)

**Task:** Build a STUB implementation that returns fake patient data. This decouples you from Fady's progress.

**Files to create:**
```
SafeDose.Infrastructure/ExternalServices/Interaction/
└── StubPatientContextProvider.cs
```

```csharp
public class StubPatientContextProvider : IPatientContextProvider
{
    public Task<PatientContextSnapshot> GetSnapshotAsync(Guid patientId)
    {
        // Hardcoded test data for development
        return Task.FromResult(new PatientContextSnapshot
        {
            PatientId = patientId,
            Age = 62,
            Gender = "male",
            IsPregnant = false,
            ChronicConditions = new[] { "diabetes_type2", "hypertension" },
            Allergies = new[] { "penicillin" },
            CurrentMedications = new[]
            {
                new MedicationSnapshot { Name = "Warfarin", ScientificName = "WARFARIN", Dose = "5 mg" },
                new MedicationSnapshot { Name = "Metformin", ScientificName = "METFORMIN", Dose = "850 mg" },
            }
        });
    }
}
```

**Why a stub:** lets you test your interaction pipeline end-to-end TODAY, without waiting for Fady. When his real Patient repository is ready, swap the DI registration to use his implementation.

**Definition of Done:**
- [ ] Returns realistic test data
- [ ] Registered in DI with comment: `// TODO: replace with FadyPatientContextProvider when Module 2 ships`

**Time:** 1 hour

---

## 1.8 Domain Service: SeverityCalculator (Day 4)

**Files to create:**
```
SafeDose.Domain/Services/Interaction/
└── SeverityCalculator.cs
```

```csharp
public class SeverityCalculator
{
    public int Calculate(SeveritySignals signals)
    {
        // Hard rules first
        if (signals.HasAllergyMatch) return 3;
        if (signals.HasCriticalPair) return 3;
        if (signals.HasPregnancyContraindication) return 3;
        
        // LLM-derived
        if (signals.SevereInteractionCount > 0) return 3;
        if (signals.ModerateInteractionCount > 0) return 2;
        
        // Default
        return 1;
    }
}

public record SeveritySignals(
    bool HasAllergyMatch,
    bool HasCriticalPair,
    bool HasPregnancyContraindication,
    int SevereInteractionCount,
    int ModerateInteractionCount,
    int MinorInteractionCount
);
```

**Definition of Done:**
- [ ] Pure function (no side effects)
- [ ] Unit tested with 10+ scenarios
- [ ] Returns 1, 2, or 3 (never throws)

**Time:** 2 hours (mostly tests)

---

## 1.9 Domain Service: AllergyCrossReactivityMatcher (Day 4)

```
SafeDose.Domain/Services/Interaction/
└── AllergyCrossReactivityMatcher.cs
```

```csharp
public class AllergyCrossReactivityMatcher
{
    private static readonly Dictionary<string, string[]> CrossReactivity = new()
    {
        ["penicillin"] = new[] { "amoxicillin", "ampicillin", "cephalexin", "cefaclor" },
        ["sulfa"] = new[] { "sulfamethoxazole", "trimethoprim", "sulfasalazine" },
        ["nsaid"] = new[] { "ibuprofen", "naproxen", "diclofenac", "aspirin" },
        // ... more
    };

    public bool MatchesAllergy(string drugScientificName, IEnumerable<string> patientAllergies)
    {
        var drug = drugScientificName.ToLowerInvariant();
        
        foreach (var allergy in patientAllergies)
        {
            var allergyLower = allergy.ToLowerInvariant();
            if (drug.Contains(allergyLower)) return true;
            if (CrossReactivity.TryGetValue(allergyLower, out var related))
            {
                if (related.Any(r => drug.Contains(r))) return true;
            }
        }
        return false;
    }
}
```

**Definition of Done:**
- [ ] Pure function
- [ ] Unit tested: penicillin allergy + amoxicillin → true
- [ ] Unit tested: no allergy + any drug → false

**Time:** 2 hours

---

## 1.10 Bare-bones API endpoint (Day 5)

**Task:** Create a minimal `InteractionsController` that returns hardcoded responses. This proves the wiring works.

**Files to create:**
```
SafeDose.API/Controllers/
└── InteractionsController.cs
```

```csharp
[ApiController]
[Route("api/interactions")]
[Authorize]
public class InteractionsController : ControllerBase
{
    [HttpPost("check")]
    public IActionResult Check([FromBody] CheckRequestDto dto)
    {
        // STUB: replace with use case in Phase 3
        return Ok(new
        {
            level = 2,
            labelArabic = "احذر",
            explanationArabic = "هذا رد مؤقت للاختبار",
            disclaimerArabic = "استشر طبيبك أو الصيدلي"
        });
    }
}
```

**Definition of Done:**
- [ ] Endpoint hit via Postman returns 200 with stub response
- [ ] Requires JWT (returns 401 without)
- [ ] Logged in app insights

**Time:** 1 hour

---

## Phase 1 deliverables checklist

By end of Week 1:
- [x] Domain entities created
- [x] EF migrations applied
- [x] CriticalPair table seeded with 25+ pairs
- [x] Repository interfaces + implementations done
- [x] Stub Patient Context Provider working
- [x] Two domain services (SeverityCalculator, AllergyMatcher) with unit tests
- [x] Stub API endpoint accessible via Postman

**Phase 1 success criterion:** You can hit `POST /api/interactions/check` with a JWT and get back a hardcoded "احذر" response. The Critical Pair table has data. Allergy matcher unit tests pass.

---

# PHASE 2 — LANGFLOW AI PIPELINE COMPLETION (Week 2)

## Goal of this phase

Complete the 4-stage agent pipeline in Langflow. Stages 1 + 3 are partially done. Build Stages 2 + 4 and wire them all together.

## 2.1 Stage 2: Patient Profile Agent Custom Component (Day 6)

**Where:** Langflow → "+ New Custom Component"

**What it does:** Calls your .NET API `/api/internal/patients/{id}/profile-snapshot` and returns structured patient context.

**Code template:**

```python
from langflow.custom import Component
from langflow.io import StrInput, SecretStrInput, MessageTextInput, Output
from langflow.schema.message import Message
import requests
import json


class PatientProfileFetcher(Component):
    display_name = "SafeDose Patient Profile"
    description = "Fetches patient profile snapshot from SafeDose .NET API"
    icon = "user"

    inputs = [
        MessageTextInput(name="patient_id", display_name="Patient ID", value=""),
        StrInput(name="api_base", display_name="SafeDose API Base", value="http://host.docker.internal:5000"),
        SecretStrInput(name="service_token", display_name="Service Token"),
    ]

    outputs = [Output(display_name="Profile JSON", name="profile", method="fetch")]

    def fetch(self) -> Message:
        try:
            patient_id = str(self.patient_id).strip()
            if not patient_id:
                return Message(text=json.dumps({"error": "patient_id required"}))
            
            url = f"{self.api_base}/api/internal/patients/{patient_id}/profile-snapshot"
            headers = {"X-Service-Token": self.service_token}
            r = requests.get(url, headers=headers, timeout=10)
            
            if r.status_code != 200:
                return Message(text=json.dumps({"error": f"API returned {r.status_code}", "body": r.text[:300]}))
            
            return Message(text=json.dumps(r.json(), ensure_ascii=False, indent=2))
        except Exception as e:
            return Message(text=json.dumps({"error": f"{type(e).__name__}: {str(e)}"}))
```

**Definition of Done:**
- [ ] Component saved and visible in Langflow sidebar
- [ ] Connects to your stub API endpoint
- [ ] Returns hardcoded test patient profile as JSON
- [ ] Error cases (timeout, 401, 500) return error JSON, not crash

**Time:** 2 hours

---

## 2.2 Build internal Patient Profile endpoint (Day 6)

**Task:** Add the controller that Langflow Stage 2 calls.

**Files to create:**
```
SafeDose.API/Controllers/Internal/
└── PatientProfileSnapshotController.cs
```

```csharp
[ApiController]
[Route("api/internal/patients")]
[ServiceTokenAuth]  // custom auth, NOT patient JWT
public class PatientProfileSnapshotController : ControllerBase
{
    private readonly IPatientContextProvider _patientContext;
    
    public PatientProfileSnapshotController(IPatientContextProvider patientContext)
    {
        _patientContext = patientContext;
    }
    
    [HttpGet("{patientId}/profile-snapshot")]
    public async Task<IActionResult> GetSnapshot(Guid patientId)
    {
        var snapshot = await _patientContext.GetSnapshotAsync(patientId);
        return Ok(snapshot);
    }
}
```

**Definition of Done:**
- [ ] Endpoint accessible via Postman with `X-Service-Token` header
- [ ] Returns stub patient data
- [ ] Returns 401 without service token
- [ ] Langflow can hit it (test the Custom Component above)

**Time:** 3 hours

---

## 2.3 Stage 4: Validation Agent (Day 7)

**Where:** Langflow → add a new Vertex AI (or Fireworks) component

**System prompt:** Use the one from `module-5-requirements.md` Part 9.5 (Validation Agent).

**Inputs to wire:**
- Output of Stage 3 (Comparison Agent draft verdict)
- System Message: the validator prompt
- Temperature: 0.1
- Max tokens: 8000

**Output:** Final JSON verdict matching the API contract (Part 8.1 of requirements doc).

**Definition of Done:**
- [ ] Component added to flow
- [ ] Connected: Stage 3 output → Stage 4 input
- [ ] System prompt pasted in full
- [ ] Returns valid JSON with all required fields (level, labelArabic, explanationArabic, sources, etc.)
- [ ] Tested with 3 inputs (manual JSON paste into Playground)

**Time:** 3 hours (mostly prompt tuning)

---

## 2.4 Wire the full 4-stage pipeline (Day 8)

**Task:** Connect all stages in Langflow:

```
Chat Input (or API trigger)
    ↓
Drug Name extraction (parse input)
    ↓
Stage 1 — Retrieval Agent (Pinecone search via SafeDoseDrugSearch component)
    ↓
Stage 2 — Patient Profile Agent (Custom Component you built)
    ↓
Combine: drug_candidates + patient_context → JSON
    ↓
Stage 3 — Comparison Agent (Vertex AI / Fireworks)
    ↓
Stage 4 — Validation Agent (Vertex AI / Fireworks)
    ↓
Chat Output (final verdict JSON)
```

**Definition of Done:**
- [ ] Flow runs end-to-end without errors
- [ ] Output is valid JSON matching the requirements API shape
- [ ] Run times: < 8 seconds end-to-end
- [ ] Test scenarios pass:
  - [ ] Add "Ibuprofen" for patient on Warfarin → Level 3
  - [ ] Add "Vitamin D" for healthy patient → Level 1
  - [ ] Add "Penicillin" for penicillin-allergic patient → Level 3
  - [ ] Add unknown drug → Level 2

**Time:** 4 hours (lots of iteration)

---

## 2.5 Save flow as JSON + commit (Day 8)

**Task:** Export the working flow and save to repo.

```
SafeDose-AI/ai/flows/
└── drug-interaction-pipeline.json
```

Also export the Custom Components:
```
SafeDose-AI/ai/custom-components/
├── safedose-drug-search.py
├── patient-profile-fetcher.py
└── (any others)
```

**Definition of Done:**
- [ ] Flow JSON committed to repo
- [ ] Custom Components committed
- [ ] README in `/ai/` folder explains: how to import, what API keys are needed, how to run

**Time:** 1 hour

---

## Phase 2 deliverables checklist

By end of Week 2:
- [x] Stage 2 Patient Profile Agent (Custom Component) built
- [x] Internal /profile-snapshot endpoint working
- [x] Stage 4 Validation Agent built
- [x] All 4 stages wired in Langflow
- [x] 4 demo scenarios pass with expected levels
- [x] Flow + components committed to repo

**Phase 2 success criterion:** Open Langflow Playground, type "Ibuprofen" for patient ID X (who has Warfarin in stub data), get back Level 3 JSON verdict in < 8 seconds.

---

# PHASE 3 — .NET INTEGRATION (Week 2-3)

## Goal of this phase

Wrap the Langflow pipeline in proper .NET code. Build the use cases, services, and Langflow HTTP client.

## 3.1 LangflowClient implementation (Day 9)

**Files to create:**
```
SafeDose.Infrastructure/ExternalServices/Interaction/
└── LangflowClient.cs
```

```csharp
public class LangflowClient : ILangflowClient
{
    private readonly HttpClient _http;
    private readonly LangflowOptions _options;
    
    public LangflowClient(HttpClient http, IOptions<LangflowOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<InteractionPipelineResult> RunInteractionFlowAsync(
        DrugCheckRequest req,
        CancellationToken ct = default)
    {
        var payload = new
        {
            input_value = JsonSerializer.Serialize(new
            {
                drug_name = req.DrugName,
                patient_id = req.PatientId.ToString()
            }),
            output_type = "text",
            input_type = "chat"
        };

        // Configure x-api-key header
        _http.DefaultRequestHeaders.Remove("x-api-key");
        _http.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);

        var response = await _http.PostAsJsonAsync(
            $"/api/v1/run/{_options.InteractionFlowId}",
            payload,
            ct);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        
        return ParseResponse(json);
    }
    
    private InteractionPipelineResult ParseResponse(string json)
    {
        // Langflow wraps the output. Navigate to outputs[0].outputs[0].results.message.text
        var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("outputs")[0]
            .GetProperty("outputs")[0]
            .GetProperty("results")
            .GetProperty("message")
            .GetProperty("text")
            .GetString();
            
        return JsonSerializer.Deserialize<InteractionPipelineResult>(text);
    }
}

public class LangflowOptions
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string InteractionFlowId { get; set; }
}
```

**Definition of Done:**
- [ ] Sends correct request to Langflow
- [ ] Parses nested response structure
- [ ] Handles errors (timeout, 401, 500)
- [ ] Configurable via appsettings.json
- [ ] Retries 3x with exponential backoff (use Polly)

**Time:** 4 hours

---

## 3.2 Application Service: Orchestrator (Day 10)

**Files to create:**
```
SafeDose.Application/Modules/Interaction/Services/
└── InteractionCheckOrchestrator.cs
```

```csharp
public class InteractionCheckOrchestrator
{
    private readonly IPatientContextProvider _patientContext;
    private readonly ICriticalPairLookup _criticalPairs;
    private readonly ILangflowClient _langflow;
    private readonly SeverityCalculator _severityCalc;
    private readonly AllergyCrossReactivityMatcher _allergyMatcher;
    private readonly ILogger<InteractionCheckOrchestrator> _logger;

    public async Task<InteractionVerdict> RunPipelineAsync(
        DrugCheckRequest req,
        CancellationToken ct = default)
    {
        // Get patient context
        var profile = await _patientContext.GetSnapshotAsync(req.PatientId);

        // HARD RULE 1: Allergy check (no LLM needed)
        if (_allergyMatcher.MatchesAllergy(req.DrugScientificName, profile.Allergies))
        {
            _logger.LogWarning("Allergy match detected for patient {PatientId}", req.PatientId);
            return InteractionVerdict.AllergyMatch(req.DrugName, profile.Allergies);
        }

        // HARD RULE 2: Critical-pair check (no LLM needed)
        foreach (var med in profile.CurrentMedications)
        {
            var pair = await _criticalPairs.FindPairAsync(req.DrugId, med.DrugId);
            if (pair != null)
            {
                _logger.LogWarning("Critical pair {DrugA} + {DrugB} detected", req.DrugName, med.Name);
                return InteractionVerdict.CriticalPair(req.DrugName, med, pair);
            }
        }

        // SOFT RULES: LLM pipeline
        try
        {
            var pipelineResult = await _langflow.RunInteractionFlowAsync(req, ct);
            return InteractionVerdict.FromPipelineResult(pipelineResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Langflow pipeline failed; falling back to Level 2");
            return InteractionVerdict.PrecautionaryLevel2(req.DrugName, "Pipeline unavailable");
        }
    }
}
```

**Definition of Done:**
- [ ] Allergy check fires before LLM
- [ ] Critical-pair check fires before LLM
- [ ] LLM failures gracefully degrade to Level 2
- [ ] All paths logged with patient ID
- [ ] Unit tested with mocked dependencies

**Time:** 5 hours

---

## 3.3 Use Cases (Day 11)

**Files to create:**
```
SafeDose.Application/Modules/Interaction/UseCases/
├── CheckInteractionUseCase.cs
├── CheckStandaloneUseCase.cs
└── GetHistoryUseCase.cs
```

```csharp
public class CheckInteractionUseCase
{
    private readonly IInteractionRepository _repo;
    private readonly InteractionCheckOrchestrator _orchestrator;
    
    public async Task<Result<InteractionVerdict>> ExecuteAsync(CheckRequest req)
    {
        // Cache lookup
        var cached = await _repo.GetCachedResultAsync(req.PatientId, req.DrugId, TimeSpan.FromHours(1));
        if (cached != null)
            return Result.Success(InteractionVerdict.FromEntity(cached));

        // Run pipeline
        var verdict = await _orchestrator.RunPipelineAsync(req);

        // Persist
        if (req.SaveResult)
        {
            var check = InteractionCheck.Create(req, verdict);
            await _repo.AddAsync(check);
        }

        return Result.Success(verdict);
    }
}
```

**Definition of Done:**
- [ ] All 3 use cases compile
- [ ] Cache hit avoids LLM call
- [ ] Standalone case skips Patient Profile stage
- [ ] All return Result<T> with explicit success/failure

**Time:** 4 hours

---

## 3.4 Complete the Controller (Day 12)

**File to modify:**
`SafeDose.API/Controllers/InteractionsController.cs`

Replace the stub with real use case calls.

```csharp
[ApiController]
[Route("api/interactions")]
[Authorize]
public class InteractionsController : ControllerBase
{
    private readonly CheckInteractionUseCase _checkInteraction;
    private readonly CheckStandaloneUseCase _checkStandalone;
    private readonly GetHistoryUseCase _getHistory;

    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckRequestDto dto)
    {
        var result = await _checkInteraction.ExecuteAsync(dto.ToRequest(User.GetAccountId()));
        return result.IsSuccess
            ? Ok(InteractionVerdictDto.FromVerdict(result.Value))
            : StatusCode(result.ErrorStatusCode, new { error = result.ErrorMessage });
    }

    [HttpPost("check-standalone")]
    public async Task<IActionResult> CheckStandalone([FromBody] StandaloneRequestDto dto) { /* ... */ }
    
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] Guid patientId, [FromQuery] int limit = 20, [FromQuery] int offset = 0) { /* ... */ }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) { /* ... */ }
}
```

**Definition of Done:**
- [ ] All 4 public endpoints implemented
- [ ] Each returns proper HTTP status codes
- [ ] Swagger UI shows all endpoints with examples
- [ ] Postman collection updated

**Time:** 3 hours

---

## 3.5 DI Registration (Day 12)

**File to create:**
```
SafeDose.API/Configuration/Modules/
└── InteractionModuleRegistration.cs
```

```csharp
public static class InteractionModuleRegistration
{
    public static IServiceCollection AddInteractionModule(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Use cases
        services.AddScoped<CheckInteractionUseCase>();
        services.AddScoped<CheckStandaloneUseCase>();
        services.AddScoped<GetHistoryUseCase>();
        
        // Application services
        services.AddScoped<InteractionCheckOrchestrator>();
        
        // Domain services
        services.AddSingleton<SeverityCalculator>();
        services.AddSingleton<AllergyCrossReactivityMatcher>();
        
        // Repositories
        services.AddScoped<IInteractionRepository, InteractionRepository>();
        services.AddScoped<ICriticalPairLookup, CriticalPairLookup>();
        
        // Patient Context — STUB for now, swap when Fady ships
        services.AddScoped<IPatientContextProvider, StubPatientContextProvider>();
        
        // External services
        services.Configure<LangflowOptions>(config.GetSection("Langflow"));
        services.AddHttpClient<ILangflowClient, LangflowClient>(client =>
        {
            client.BaseAddress = new Uri(config["Langflow:BaseUrl"]);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy());
        
        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(3, retryAttempt - 1)));
}
```

**Definition of Done:**
- [ ] All Module 5 services registered
- [ ] HttpClient configured with timeout + retry
- [ ] LangflowOptions bound from appsettings
- [ ] App starts without DI errors

**Time:** 2 hours

---

## Phase 3 deliverables checklist

By end of Week 2-3:
- [x] LangflowClient implementation done with retries
- [x] Orchestrator combines hard rules + LLM
- [x] 3 use cases implemented
- [x] Controller with all 4 endpoints
- [x] DI registration complete

**Phase 3 success criterion:** Hit `POST /api/interactions/check` via Postman with real patient ID + drug ID. Response comes back with proper level, sources, Arabic explanation. Cache works (second identical call returns instantly).

---

# PHASE 4 — TESTING (Week 3)

## 4.1 Unit tests (Day 13-14)

**Files to create:**
```
tests/SafeDose.UnitTests/Interaction/
├── SeverityCalculatorTests.cs
├── AllergyMatcherTests.cs
├── OrchestratorTests.cs
└── CheckInteractionUseCaseTests.cs
```

**Approach:** xUnit + Moq + FluentAssertions.

**Sample test:**
```csharp
[Fact]
public async Task Check_PatientOnWarfarin_AddsIbuprofen_ReturnsLevel3()
{
    // Arrange
    var orchestrator = SetupMockedOrchestrator(
        criticalPair: ("Warfarin", "Ibuprofen", "Bleeding risk")
    );
    
    // Act
    var verdict = await orchestrator.RunPipelineAsync(new DrugCheckRequest
    {
        DrugName = "Ibuprofen",
        DrugScientificName = "IBUPROFEN",
        PatientId = Guid.NewGuid()
    });
    
    // Assert
    verdict.Level.Should().Be(3);
    verdict.LabelArabic.Should().Be("خطر");
    verdict.ConflictingDrugs.Should().ContainSingle(d => d.Name == "Warfarin");
}
```

**Definition of Done:**
- [ ] At least 20 unit tests across the 4 classes
- [ ] All 10 demo scenarios from requirements doc covered
- [ ] Coverage > 70% in Application + Domain layers

**Time:** 8 hours

---

## 4.2 Integration tests (Day 15)

**File:** `tests/SafeDose.IntegrationTests/InteractionIntegrationTests.cs`

Test against a REAL SQL database + REAL Langflow (or mocked Langflow with WireMock).

**Definition of Done:**
- [ ] End-to-end test: POST /check returns proper response from real DB + Langflow
- [ ] History endpoint returns saved checks
- [ ] Cache works (second call < 200ms)
- [ ] Critical-pair check triggers without LLM

**Time:** 6 hours

---

## 4.3 Manual demo scenarios test (Day 16)

Run all 10 scenarios from `module-5-requirements.md` Part 13.3.

**Track results:**

| Scenario | Expected | Actual | Pass/Fail |
|----------|----------|--------|-----------|
| Hypertension + Ibuprofen | Level 2 | ? | ? |
| Warfarin + Ibuprofen | Level 3 | ? | ? |
| Penicillin allergy + Augmentin | Level 3 | ? | ? |
| Diabetic + Crestor | Level 1 | ? | ? |
| Pregnant + Isotretinoin | Level 3 | ? | ? |
| Healthy + Vitamin D | Level 1 | ? | ? |
| Standalone Warfarin + Aspirin | Level 3 | ? | ? |
| Standalone Paracetamol + Vitamin C | Level 1 | ? | ? |
| Unknown drug | Level 2 | ? | ? |
| History of Level 3 check | Visible | ? | ? |

**Definition of Done:**
- [ ] 10/10 scenarios pass
- [ ] Results documented in test report

**Time:** 2 hours

---

# PHASE 5 — FRONTEND INTEGRATION (Week 4)

## Coordination point with Doaa

Send Doaa:
1. Swagger URL (your API)
2. Postman collection with example requests
3. Arabic text catalog (levels, disclaimers, messages)
4. Wireframes / your UI screenshot for the result screen

**Doaa builds:**
- Add drug screen (autocomplete + check button)
- Result screen (full-screen colored card)
- History list
- Detail view

**Your job during this phase:**
- Be available for API questions
- Fix any API contract mismatches
- Tune Arabic phrasing based on her UI feedback

**Time:** 1 week with Doaa, your involvement is part-time

---

# PHASE 6 — DEMO PREP (Week 5)

## 6.1 Real-prescription testing

Get 5 real prescriptions (anonymized). Run them through the pipeline. Verify outputs make medical sense.

## 6.2 Arabic phrasing review

Show outputs to a native speaker (your family). Refine messages to be:
- Clear
- Not too clinical
- Not patronizing
- Action-oriented

## 6.3 Performance tuning

Measure:
- p50 response time: target < 5s
- p95 response time: target < 8s
- p99 response time: target < 12s

Optimizations if needed:
- Increase Pinecone top_k batch
- Cache hot drug pairs
- Pre-warm Langflow on app startup

## 6.4 Demo rehearsal

Run through the supervisor demo flow:
1. Login (Andrew's auth)
2. Set up patient profile (Fady's, or use seeded test patient)
3. Upload prescription photo (Ahmed's parser)
4. See OCR results
5. Confirm medications → interaction check fires
6. See severity warnings
7. View history

**Time:** 1 week

---

# COORDINATION TIMELINE

```
Week 1: You build foundation (entities, repos, stubs)
        Andrew: helps with Profile if Fady is overloaded
        Fady: starts Patient Profile module
        Ahmed: continues Prescription Parser API
        Doaa: frontend foundations (shared components)

Week 2: You build Langflow stages 2 + 4
        You build .NET integration
        Fady: ships Patient Profile API (you swap stub → real)
        Ahmed: ships Prescription confirm endpoint
        Doaa: M1+M2 frontend (Auth + Profile screens)

Week 3: Testing across all your work
        You + Doaa: API contract review
        Fady, Ahmed: continue their modules

Week 4: Frontend wiring with Doaa
        You: support + bug fixes
        Demo flow rehearsal

Week 5: Polish, Arabic review, supervisor demo
```

---

# DAILY HABITS

To stay on track:

1. **Every morning:** check this doc, pick today's unchecked task, mark it in_progress
2. **Every evening:** check completed tasks, push code to GitHub
3. **End of week:** review phase deliverables checklist; if any unchecked, plan recovery
4. **Block 2-hour focus blocks** for coding; avoid context switching
5. **Use the requirements doc IDs** when committing: `git commit -m "FR-201 + FR-202: pipeline orchestration"`

---

# DEFINITION OF MODULE 5 DONE

Use this final checklist for sign-off:

## Must-haves (P0)
- [ ] All 4 Langflow stages built and wired
- [ ] Patient Profile Agent calls .NET API
- [ ] All 5 .NET endpoints documented in Swagger
- [ ] Critical-pair table seeded with 25+ pairs
- [ ] Allergy hard-rule fires correctly
- [ ] Critical-pair hard-rule fires correctly
- [ ] LLM pipeline returns proper JSON
- [ ] All 10 demo scenarios pass
- [ ] Frontend integrated by Doaa
- [ ] Demo flow works end-to-end on a phone

## Should-haves (P1)
- [ ] p95 response time < 8s
- [ ] Cache hit rate > 30% in load test
- [ ] Background re-verification job working
- [ ] Unit test coverage > 70%
- [ ] Integration tests for all endpoints

## Nice-to-haves (P2)
- [ ] Real-prescription testing with 5 cases
- [ ] Arabic phrasing reviewed by native speaker
- [ ] Performance dashboard set up

---

# RISKS TO WATCH

| Risk | Watch for | Action |
|------|-----------|--------|
| Fady's Patient Profile late | End of Week 2 | Keep stub working, swap later |
| Langflow API quota hit | Daily check | Switch to Fireworks for Validator |
| LLM hallucinates wrong drug interactions | E2E test results | Tighten validator prompt, add more critical pairs |
| Pinecone slow on demo day | Pre-demo test | Pre-warm + cache common queries |
| Doaa frontend not ready | End of Week 4 | Stub frontend for demo, show via Postman + screens mock |

---

# END

Update this doc as you go. Cross off completed tasks. Add notes when you hit edges. By end of Week 5, every checkbox should be marked.

— Mina
