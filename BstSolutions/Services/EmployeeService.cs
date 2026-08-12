using BstSolutions.Common;
using BstSolutions.Models;
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

    public async Task<List<EmployeeListItemViewModel>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(MapToListItem).ToList();
    }

    public async Task<List<EmployeeListItemViewModel>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Where(e => e.IsActive).Select(MapToListItem).ToList();
    }

    public async Task<EditEmployeeViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        return new EditEmployeeViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            IsActive = employee.IsActive
        };
    }

    public async Task CreateAsync(CreateEmployeeViewModel model, CancellationToken cancellationToken = default)
    {
        var email = model.Email.Trim();

        if (await _employeeRepository.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            throw new BusinessException(
                "An employee with this email already exists.",
                "EMPLOYEE_EMAIL_EXISTS");
        }

        var employee = new Employee
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = email,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EditEmployeeViewModel model, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(model.Id, cancellationToken)
            ?? throw new BusinessException(
                "Employee not found.",
                "EMPLOYEE_NOT_FOUND");

        var email = model.Email.Trim();

        if (await _employeeRepository.EmailExistsAsync(email, model.Id, cancellationToken))
        {
            throw new BusinessException(
                "An employee with this email already exists.",
                "EMPLOYEE_EMAIL_EXISTS");
        }

        employee.FirstName = model.FirstName.Trim();
        employee.LastName = model.LastName.Trim();
        employee.Email = email;
        employee.IsActive = model.IsActive;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);
    }

    private static EmployeeListItemViewModel MapToListItem(Employee employee)
    {
        return new EmployeeListItemViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            CreatedDate = employee.CreatedDate
        };
    }
}
