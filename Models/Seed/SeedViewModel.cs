namespace AccuFlow.Models.Seed
{
    public class SeedViewModel
    {
        public bool IsProduction { get; set; }
        public List<string> AvailableSeeders { get; set; } = new List<string>();
        public string? Message { get; set; }
    }
}
