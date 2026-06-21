# خطة تنفيذ عرض الروشتات (Prescriptions Viewer)

بناءً على الصورتين، الـ Frontend محتاج 2 Endpoints أساسيين:
1. **Endpoint لعرض قائمة الروشتات (Summary View)**: بتعرض اسم الروشتة، تاريخها، عدد الأدوية، وأسماء الأدوية كـ Tags (الصورة التانية).
2. **Endpoint لعرض تفاصيل الروشتة (Detailed View)**: بتعرض كل دواء بتفاصيله (الجرعة، التكرار، المدة) (الصورة الأولى).

---

## 1. تصميم الـ DTOs (Data Transfer Objects)

هنعمل فولدر/ملفات للـ DTOs الخاصة بالروشتات:

```csharp
// لعرض القائمة المبسطة (الصورة التانية)
public class PrescriptionSummaryDto
{
    public int PrescriptionId { get; set; }
    public string PrescriptionName { get; set; }
    public string Date { get; set; } // e.g., "15/05/2025"
    public int DrugCount { get; set; }
    public List<string> DrugNames { get; set; } // ["أملوديبين", "ميتفورمين", "بانادول اكسترا"]
}

// لعرض التفاصيل الكاملة (الصورة الأولى)
public class PrescriptionDetailDto
{
    public int PrescriptionId { get; set; }
    public string PrescriptionName { get; set; }
    public string Date { get; set; }
    public int DrugCount { get; set; }
    public List<PrescriptionMedicationDto> Medications { get; set; }
}

public class PrescriptionMedicationDto
{
    public string DrugNameAr { get; set; } // "بانادول اكسترا"
    public string DrugNameEn { get; set; } // "Paracetamol + Caffeine"
    public string Dose { get; set; } // "500 ملغ - قرص واحد"
    public string Frequency { get; set; } // "3 مرات يومياً"
    public string Duration { get; set; } // "لمدة 5 أيام" أو "مستمر"
}
```

---

## 2. إنشاء الـ Use Cases (طبقة الـ Application)

هنضيف 2 Use Cases لتنفيذ الـ Business Logic:

### أ. `GetPatientPrescriptionsUseCase`
- **الوظيفة**: بتستقبل `patientId` و `accountId` (للتأكد من الصلاحيات).
- **الاستعلام**: بتجيب كل الـ `Prescription` الخاصة بالمريض مع عمل `Include` للـ `Drugs` أو الـ `PatientMedications`.
- **التحويل**: بتعبي الـ `PrescriptionSummaryDto`.

### ب. `GetPrescriptionDetailsUseCase`
- **الوظيفة**: بتستقبل `prescriptionId` و `accountId`.
- **الاستعلام**: بتجيب الروشتة بـ `Include` للـ `PatientMedications` والـ `Drug` المرتبط والـ `DrugCatalog` (عشان نجيب الاسم التجاري والعلمي ورمز السجل لو متاح).
- **حساب المدة (Duration)**: هنحسبها من الـ `StartDate` والـ `EndDate` الموجودين في جدول `PatientMedication`.
- **التعارضات (Interactions)**: هنستعلم من جدول `InteractionCheck` لو فيه أي تحذيرات مرتبطة بالأدوية دي عشان نعرضها في `InteractionWarning`.

---

## 3. تعديل الـ Controllers (طبقة الـ API)

هننشئ `PrescriptionController` (أو نضيف على موجود لو عندك) ويكون فيه الـ Endpoints دي:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionController : ControllerBase
{
    [HttpGet("Patient/{patientId}/Summary")]
    public async Task<IActionResult> GetPrescriptionsSummary(int patientId, [FromServices] GetPatientPrescriptionsUseCase useCase)
    {
        // استخراج accountId من الـ User Token
        // استدعاء הـ useCase
        // إرجاع النتيجة
    }

    [HttpGet("{prescriptionId}/Details")]
    public async Task<IActionResult> GetPrescriptionDetails(int prescriptionId, [FromServices] GetPrescriptionDetailsUseCase useCase)
    {
        // استخراج accountId من الـ User Token
        // استدعاء הـ useCase
        // إرجاع النتيجة
    }
}
```

---

> [!IMPORTANT]
> **أسئلة مفتوحة (Open Questions) للاتفاق عليها قبل التنفيذ**:
> 1. بالنسبة للـ **Frequency (التكرار)**، هو متسجل في الداتابيز كـ `int` (مثلاً 3). بما إنك حطيت نوعه `string` في الـ DTO، هل تحب أعمل Logic في الـ Backend يحوله لنص زي "3 مرات يومياً" مباشرة؟
> 2. نفس الكلام للـ **Duration (المدة)**، هل تحب نحسبها من تاريخ البداية والنهاية ونرجعها كنص "لمدة 5 أيام" مثلاً؟

في انتظار تأكيدك على النقطتين دول عشان أبدأ في كتابة الكود على طول!
