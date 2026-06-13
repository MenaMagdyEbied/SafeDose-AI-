using SafeDose.Application.DTOs.PrescriptionDTOs;

namespace SafeDose.Application.Interfaces;

public interface ILangflowPrescriptionClient
{
    Task<ParsedPrescriptionDto> ParsePrescriptionAsync(Stream imageStream, string fileName, string contentType);
    
    Task<ParsedPrescriptionDto> ParsePrescriptionByUrlAsync(string imageUrl);
}
