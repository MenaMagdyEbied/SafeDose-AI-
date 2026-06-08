namespace SafeDose.Shared.Errors;

// Unified error shape returned by ALL controllers.
// Doaa can write ONE error-handling component on the frontend.
public record ErrorResponse(
    string Code,
    string MessageArabic,
    string? MessageEnglish = null,
    string? Details = null
);

public static class ErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string NotFound         = "NOT_FOUND";
    public const string Unauthorized     = "UNAUTHORIZED";
    public const string Forbidden        = "FORBIDDEN";
    public const string TooManyDrugs     = "TOO_MANY_DRUGS";
    public const string DrugNotFound     = "DRUG_NOT_FOUND";
    public const string LangflowUnavail  = "LANGFLOW_UNAVAILABLE";
    public const string ServerError      = "SERVER_ERROR";
}

public static class ArabicMessages
{
    public const string TooManyDrugs       = "الحد الأقصى ستة أدوية في الفحص الواحد";
    public const string DrugNotFound       = "أحد الأدوية المحددة غير موجود في قاعدة البيانات";
    public const string ValidationFailed   = "البيانات المدخلة غير صحيحة";
    public const string CheckNotFound      = "نتيجة الفحص غير موجودة";
    public const string PatientNotFound    = "المريض غير موجود";
    public const string Unauthorized       = "يجب تسجيل الدخول أولاً";
    public const string Forbidden          = "غير مصرح لك بهذا الإجراء";
    public const string ServerError        = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً";
}
