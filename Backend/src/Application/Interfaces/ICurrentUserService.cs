using System;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Guid? CompanyId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }
}
