using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public int? DoctorId { get; set; }
    public int? StudentId { get; set; }
}
