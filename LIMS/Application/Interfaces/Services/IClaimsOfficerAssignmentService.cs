namespace Application.Interfaces.Services;

public interface IClaimsOfficerAssignmentService
{
    Task<int> AssignOfficerAsync(); // returns assigned ClaimsOfficerId
}