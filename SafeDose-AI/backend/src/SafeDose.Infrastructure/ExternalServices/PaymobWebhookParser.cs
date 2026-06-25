using System.Text.Json;
using SafeDose.Application.UseCases.Billing;

namespace SafeDose.Infrastructure.ExternalServices;

// Parses Paymob's TRANSACTION processed callback (POST JSON + ?hmac= query param).
public static class PaymobWebhookParser
{
    public static bool TryParse(string jsonBody, string? hmacFromQuery, out PaymobWebhookPayload? payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(jsonBody))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(jsonBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeEl)
                || !string.Equals(typeEl.GetString(), "TRANSACTION", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!root.TryGetProperty("obj", out var obj))
                return false;

            var concatenated = BuildTransactionHmacString(obj);
            var orderId = obj.TryGetProperty("order", out var orderEl) && orderEl.TryGetProperty("id", out var orderIdEl)
                ? orderIdEl.GetRawText().Trim('"')
                : string.Empty;

            var transactionId = obj.TryGetProperty("id", out var idEl)
                ? idEl.GetRawText().Trim('"')
                : string.Empty;

            var success = obj.TryGetProperty("success", out var successEl) && successEl.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => string.Equals(successEl.GetString(), "true", StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            var amountCents = obj.TryGetProperty("amount_cents", out var amountEl)
                ? amountEl.GetDecimal()
                : 0m;

            var currency = obj.TryGetProperty("currency", out var currencyEl)
                ? currencyEl.GetString() ?? "EGP"
                : "EGP";

            // Try to extract a failure reason if present in Paymob's object
            string? failure = null;
            if (obj.TryGetProperty("failure", out var failEl))
                failure = failEl.ValueKind == JsonValueKind.String ? failEl.GetString() : failEl.GetRawText();
            if (string.IsNullOrWhiteSpace(failure) && obj.TryGetProperty("failure_code", out var fc))
                failure = fc.GetRawText().Trim('"');
            if (string.IsNullOrWhiteSpace(failure) && obj.TryGetProperty("failure_message", out var fm))
                failure = fm.GetRawText().Trim('"');

            payload = new PaymobWebhookPayload(
                transactionId,
                orderId,
                success,
                amountCents,
                currency,
                hmacFromQuery ?? string.Empty,
                concatenated,
                failure);

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Field order per Paymob Accept legacy TRANSACTION callback docs.
    private static string BuildTransactionHmacString(JsonElement obj)
    {
        var orderId = obj.TryGetProperty("order", out var orderEl) && orderEl.TryGetProperty("id", out var orderIdEl)
            ? orderIdEl.GetRawText().Trim('"')
            : string.Empty;

        var sourcePan = string.Empty;
        var sourceType = string.Empty;
        var sourceSubType = string.Empty;
        if (obj.TryGetProperty("source_data", out var sourceEl))
        {
            if (sourceEl.TryGetProperty("pan", out var panEl))
                sourcePan = panEl.GetString() ?? string.Empty;
            if (sourceEl.TryGetProperty("type", out var typeEl))
                sourceType = typeEl.GetString() ?? string.Empty;
            if (sourceEl.TryGetProperty("sub_type", out var subTypeEl))
                sourceSubType = subTypeEl.GetString() ?? string.Empty;
        }

        return string.Concat(
            GetRaw(obj, "amount_cents"),
            GetString(obj, "created_at"),
            GetString(obj, "currency"),
            BoolString(obj, "error_occured"),
            BoolString(obj, "has_parent_transaction"),
            GetRaw(obj, "id"),
            GetRaw(obj, "integration_id"),
            BoolString(obj, "is_3d_secure"),
            BoolString(obj, "is_auth"),
            BoolString(obj, "is_capture"),
            BoolString(obj, "is_refunded"),
            BoolString(obj, "is_standalone_payment"),
            BoolString(obj, "is_voided"),
            orderId,
            GetRaw(obj, "owner"),
            BoolString(obj, "pending"),
            sourcePan,
            sourceSubType,
            sourceType,
            BoolString(obj, "success"));
    }

    private static string GetString(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var el) ? el.GetString() ?? string.Empty : string.Empty;

    private static string GetRaw(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var el) ? el.GetRawText().Trim('"') : string.Empty;

    private static string BoolString(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var el))
            return "false";

        return el.ValueKind switch
        {
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.String => string.Equals(el.GetString(), "true", StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false",
            _ => "false"
        };
    }
}
