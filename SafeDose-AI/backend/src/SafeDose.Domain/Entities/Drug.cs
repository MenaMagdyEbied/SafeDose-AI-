namespace SafeDose.Domain.Entities;

public class Drug
{
    public int DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public byte Route { get; set; }
    public string? Dose { get; set; }
}
