namespace AccuFlow.Models.BaseModel
{
    public class BaseDatatableResponse
    {
        public int? Draw { get; set; } = 0;
        public int? RecordsTotal { get; set; } = 0;
        public int? RecordsFiltered { get; set; } = 0;
        public object? Data { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
        public bool IsSuccess { get; set; } = true;
    }
}
