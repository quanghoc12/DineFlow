using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Services.Auth;

public interface IUserService
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
}
