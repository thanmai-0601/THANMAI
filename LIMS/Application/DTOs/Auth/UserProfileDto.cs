using Domain.Enums;

namespace Application.DTOs.Auth
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bank Details
        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        
        // Bank Details
        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }
    }
}
