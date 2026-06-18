using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;
using SafeDose.Domain.ApplicationDbContext;

namespace SafeDose.Application.UseCases
{
    public class GenerateQrCodeUseCase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public GenerateQrCodeUseCase(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<byte[]> ExecuteAsync(int patientId, string accountId)
        {
            var patient = await _db.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId && p.AccountId == accountId);

            if (patient == null || !patient.IsActive)
            {
                throw new KeyNotFoundException("Patient not found or inactive.");
            }

            // Build the Public Emergency URL
            // Read the base URL from appsettings, or fallback to a placeholder for Swagger testing
            var frontendBaseUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var emergencyUrl = $"{frontendBaseUrl.TrimEnd('/')}/emergency-card/{patient.MedicalCardToken}";

            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(emergencyUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
          
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            return qrCodeImage; 
        }
    }
}
