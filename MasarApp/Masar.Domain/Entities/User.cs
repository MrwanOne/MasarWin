using Masar.Domain.Enums;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;

    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
}
