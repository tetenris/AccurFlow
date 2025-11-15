namespace AccuFlow.Entities.Abstractions
{
    public interface IEntity
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        bool IsDeleted { get; set; }
    }
}
