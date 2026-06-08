using Microsoft.AspNetCore.Mvc;
using SafeDose.Api.Auth;
using SafeDose.Application.UseCases;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers.Internal;

// Internal endpoint — called by Langflow Stage 2 (Patient Profile Agent).
// Authenticated with X-Service-Token header, NOT patient JWT.
// Do NOT expose this through the public API gateway.
[ApiController]
[Route("api/internal/patients")]
[ServiceToken]
public class PatientProfileSnapshotController : ControllerBase
{
    private readonly GetPatientProfileSnapshotUseCase _useCase;

    public PatientProfileSnapshotController(GetPatientProfileSnapshotUseCase useCase)
    {
        _useCase = useCase;
    }

    // GET /api/internal/patients/{patientId}/profile-snapshot
    // Header: X-Service-Token: <service-token>
    // Returns: PatientProfileSnapshotDto with age, conditions, allergies, current meds
    [HttpGet("{patientId:int}/profile-snapshot")]
    public async Task<IActionResult> GetSnapshot(
        int patientId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _useCase.ExecuteAsync(patientId, cancellationToken);
        if (snapshot == null)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, ArabicMessages.PatientNotFound));

        return Ok(snapshot);
    }
}
