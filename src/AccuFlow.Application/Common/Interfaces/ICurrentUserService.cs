namespace AccuFlow.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserName { get; }
        string FullName { get; }
        string Email { get; }
        Guid RoleId { get; }
        string RoleName { get; }
    }
}