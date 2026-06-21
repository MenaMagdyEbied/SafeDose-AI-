using System;
using System.Collections.Generic;

namespace SafeDose.Application.DTOs.PrescriptionDTOs
{
    public class PrescriptionSummaryDto
    {
        public int PrescriptionId { get; set; }
        public string PrescriptionName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int DrugCount { get; set; }
        public List<string> DrugNames { get; set; } = new List<string>();
    }
}
