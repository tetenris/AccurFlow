namespace AccuFlow.Infrastructures
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

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId => GetClaimValue<Guid>("UserId");
        public string UserName => GetClaimValue<string>("UserName") ?? string.Empty;
        public string FullName => GetClaimValue<string>("FullName") ?? string.Empty;
        public string Email => GetClaimValue<string>("Email") ?? string.Empty;
        public Guid RoleId => GetClaimValue<Guid>("RoleId");
        public string RoleName => GetClaimValue<string>("RoleName") ?? string.Empty;

        private T GetClaimValue<T>(string claimType)
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType);
            if (claim == null) return default(T)!;

            try
            {
                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default(T)!;
            }
        }
    }
}
