using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

// soft delete (deactivate) an owned Patient.
// Never hard delete - medical record retention.
public class DeactivatePatientUseCase
{
    private readonly IPatientRepository _patients;
    private readonly IAuditLogService _audit;
    private readonly AppDbContext _context;
    private readonly IUserGlobalServices _userGlobalServices;
    public DeactivatePatientUseCase(IPatientRepository patients, IAuditLogService audit, AppDbContext context , IUserGlobalServices userGlobalServices)
    {
        _patients = patients;
        _audit = audit;
        _context= context;
        _userGlobalServices = userGlobalServices;
    }

    public async Task<bool> ExecuteAsync(
        int patientId,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        Account account = await _userGlobalServices.GetUser();
        Patient? patient = await _context.Patients.Where(p => p.PatientId == patientId && p.AccountId == account.Id && p.IsRunning == true).SingleOrDefaultAsync();
        if (patient != null)
            throw new Exception("انه المريض المفعل ولا يمكن حزفه");

        // Ownership check via repo helper (works even on already-inactive)
        var owns = await _patients.ExistsForAccountAsync(patientId, accountId);
        if (!owns) return false;

        await _patients.SoftDeleteAsync(patientId);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Patient),
            EntityRowId: patientId,
            ActionType: 4,                              // 4 = Delete (soft)
            AccessReason: "Patient soft-deleted by user"
        ), cancellationToken);

        return true;
    }
}
