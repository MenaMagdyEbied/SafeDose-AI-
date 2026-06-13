namespace SafeDose.Domain.Entities
{
    public class DrugCatalog
    {
        public int DrugCatalogId { get; set; }
        public string CommercialNameEn { get; set; } = null!;
        public string? CommercialNameAr { get; set; }
        public string? ScientificName { get; set; }
        public string? Manufacturer { get; set; }
        public string? DrugClass { get; set; }
        public string? Route { get; set; }
        public decimal? PriceEgp { get; set; }
    }
}
