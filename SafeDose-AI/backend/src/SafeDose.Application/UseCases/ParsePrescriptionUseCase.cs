using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class ParsePrescriptionUseCase
{
    private readonly ILangflowPrescriptionClient _langflowClient;

    public ParsePrescriptionUseCase(ILangflowPrescriptionClient langflowClient)
    {
        _langflowClient = langflowClient;
    }

    public async Task<ParsedPrescriptionDto> ExecuteAsync(Stream imageStream, string fileName, string contentType)
    {
        if (imageStream == null || imageStream.Length == 0)
        {
            throw new ArgumentException("Prescription image stream is required.");
        }

        return await _langflowClient.ParsePrescriptionAsync(imageStream, fileName, contentType);
    }
}
