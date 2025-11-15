namespace AccuFlow.Models.BaseModel
{
    public class BaseDatatableRequest
    {
        public int Draw { get; set; }
        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public string? OrderType { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }
}
