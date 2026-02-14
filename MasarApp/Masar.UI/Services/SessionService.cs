using Masar.Application.DTOs;

namespace Masar.UI.Services;

public class SessionService : ISessionService
{
    public UserDto? CurrentUser { get; private set; }

    public void SetUser(UserDto user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
