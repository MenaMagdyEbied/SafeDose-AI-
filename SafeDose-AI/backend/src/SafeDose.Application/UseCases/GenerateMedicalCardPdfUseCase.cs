using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SafeDose.Application.UseCases
{
    public class GenerateMedicalCardPdfUseCase
    {
        private readonly GetPrivateMedicalCardUseCase _getPrivateMedicalCardUseCase;

        public GenerateMedicalCardPdfUseCase(GetPrivateMedicalCardUseCase getPrivateMedicalCardUseCase)
        {
            _getPrivateMedicalCardUseCase = getPrivateMedicalCardUseCase;
        }

        public async Task<byte[]> ExecuteAsync(int patientId, string accountId)
        {
            // Set QuestPDF license context
            QuestPDF.Settings.License = LicenseType.Community;

            // Fetch patient data using the existing UseCase to ensure security and logic reusability
            var cardDto = await _getPrivateMedicalCardUseCase.ExecuteAsync(patientId, accountId);

            // Generate PDF document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, cardDto));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("SafeDose Medical Card").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                });
                
                row.ConstantItem(100).AlignRight().Text($"Date: {DateTime.Now:d}").FontSize(10);
            });
        }

        private void ComposeContent(IContainer container, DTOs.MedicalCardDto cardDto)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                // Patient Information Section
                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(infoColumn =>
                {
                    infoColumn.Spacing(5);
                    infoColumn.Item().Text($"Name: {cardDto.FullName}").FontSize(14).SemiBold();
                    
                    if (cardDto.DateOfBirth.HasValue)
                    {
                        var age = DateTime.Today.Year - cardDto.DateOfBirth.Value.Year;
                        infoColumn.Item().Text($"Age: {age} Years (DOB: {cardDto.DateOfBirth.Value:d})");
                    }
                    
                    infoColumn.Item().Text($"Gender: {cardDto.Gender}");
                    infoColumn.Item().Text($"Blood Type: {cardDto.BloodType}");
                    infoColumn.Item().Text($"Allergies: {cardDto.Allergies}").FontColor(Colors.Red.Medium);
                    infoColumn.Item().Text($"Chronic Conditions: {cardDto.ChronicConditions}");
                });

                // Medications Section
                column.Item().Text("Current Medications").FontSize(16).SemiBold();
                
                column.Item().Table(table =>
                {
                    // columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Drug Name
                        columns.RelativeColumn(2); // Dose
                        columns.RelativeColumn(2); // Frequency
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Drug Name");
                        header.Cell().Element(CellStyle).Text("Dose");
                        header.Cell().Element(CellStyle).Text("Frequency (per day)");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    
                    if (cardDto.CurrentMedications == null || !cardDto.CurrentMedications.Any())
                    {
                        table.Cell().ColumnSpan(3).PaddingVertical(5).Text("No active medications found.");
                    }
                    else
                    {
                        foreach (var med in cardDto.CurrentMedications)
                        {
                            table.Cell().Element(CellStyle).Text(med.DrugName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(med.Dose ?? "N/A");
                            table.Cell().Element(CellStyle).Text(med.Frequency?.ToString() ?? "N/A");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        }
                    }
                });
            });
        }
    }
}
