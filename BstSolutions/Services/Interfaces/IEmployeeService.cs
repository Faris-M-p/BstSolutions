using BstSolutions.ViewModels.Employee;

namespace BstSolutions.Services.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeListItemViewModel>> GetEmployeesAsync(CancellationToken cancellationToken = default);

    Task<List<EmployeeListItemViewModel>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);

    Task<EditEmployeeViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateEmployeeViewModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(EditEmployeeViewModel model, CancellationToken cancellationToken = default);
}
