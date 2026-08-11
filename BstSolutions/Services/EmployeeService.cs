using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Employee;

namespace BstSolutions.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public Task<List<EditEmployeeViewModel>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task<EditEmployeeViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task CreateAsync(CreateEmployeeViewModel model, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task UpdateAsync(EditEmployeeViewModel model, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }
}
