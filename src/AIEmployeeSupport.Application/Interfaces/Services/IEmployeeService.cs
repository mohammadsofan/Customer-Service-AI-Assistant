using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Employees;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<PaginatedResponse<EmployeeListDto>> GetAllAsync(PaginatedRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeListDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeListDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
