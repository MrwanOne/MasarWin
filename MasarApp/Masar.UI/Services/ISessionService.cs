using Masar.Application.DTOs;

namespace Masar.UI.Services;

public interface ISessionService
{
    UserDto? CurrentUser { get; }
    void SetUser(UserDto user);
    void Clear();
}
