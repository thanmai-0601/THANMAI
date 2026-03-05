using Domain.Enums;

namespace Application.DTOs.Dashboard;

public class UserRoleCountDto
{
    public UserRole Role { get; set; }
    public int Count { get; set; }
}