using Masar.Application.Interfaces;
using Masar.Domain.Enums;

namespace Masar.UI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ISessionService _sessionService;

    public CurrentUserService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public int? UserId => _sessionService.CurrentUser?.UserId;
    public string? Username => _sessionService.CurrentUser?.Username;
    public UserRole? Role => _sessionService.CurrentUser?.Role;
    public bool IsAuthenticated => _sessionService.CurrentUser != null;
}
