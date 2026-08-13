using BstSolutions.Common;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services;
using BstSolutions.Tests.Helpers;
using Moq;

namespace BstSolutions.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _employeeService = new EmployeeService(_employeeRepositoryMock.Object);
    }

    [Fact]
    public async Task GetEmployeesAsync_ReturnsMappedEmployees()
    {
        var employees = new List<Employee>
        {
            TestDataFactory.CreateEmployee(1, "John", "Doe", "john@example.com"),
            TestDataFactory.CreateEmployee(2, "Jane", "Smith", "jane@example.com", isActive: false)
        };

        _employeeRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var result = await _employeeService.GetEmployeesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("John", result[0].FirstName);
        Assert.Equal("jane@example.com", result[1].Email);
    }

    [Fact]
    public async Task GetActiveEmployeesAsync_ReturnsOnlyActiveEmployees()
    {
        var employees = new List<Employee>
        {
            TestDataFactory.CreateEmployee(1, isActive: true),
            TestDataFactory.CreateEmployee(2, "Jane", "Smith", "jane@example.com", isActive: false)
        };

        _employeeRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var result = await _employeeService.GetActiveEmployeesAsync();

        Assert.Single(result);
        Assert.True(result[0].IsActive);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEditModel()
    {
        var employee = TestDataFactory.CreateEmployee(5, "Alice", "Brown", "alice@example.com");

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _employeeService.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeMissing_ReturnsNull()
    {
        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var result = await _employeeService.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailIsUnique_AddsEmployeeAndSaves()
    {
        var model = TestDataFactory.CreateEmployeeModel();

        _employeeRepositoryMock
            .Setup(r => r.EmailExistsAsync(model.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _employeeService.CreateAsync(model);

        _employeeRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Employee>(e =>
                    e.FirstName == "John" &&
                    e.LastName == "Doe" &&
                    e.Email == "john.doe@example.com" &&
                    e.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _employeeRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_ThrowsBusinessException()
    {
        var model = TestDataFactory.CreateEmployeeModel(email: "taken@example.com");

        _employeeRepositoryMock
            .Setup(r => r.EmailExistsAsync("taken@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _employeeService.CreateAsync(model));

        Assert.Equal("EMPLOYEE_EMAIL_EXISTS", ex.ErrorCode);
        _employeeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeExists_UpdatesAndSaves()
    {
        var existing = TestDataFactory.CreateEmployee(1);
        var model = TestDataFactory.EditEmployeeModel(
            id: 1,
            firstName: "Johnny",
            lastName: "Updated",
            email: "johnny@example.com",
            isActive: false);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _employeeRepositoryMock
            .Setup(r => r.EmailExistsAsync("johnny@example.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _employeeService.UpdateAsync(model);

        Assert.Equal("Johnny", existing.FirstName);
        Assert.Equal("johnny@example.com", existing.Email);
        Assert.False(existing.IsActive);

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once);
        _employeeRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeMissing_ThrowsNotFoundException()
    {
        var model = TestDataFactory.EditEmployeeModel(id: 99);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _employeeService.UpdateAsync(model));

        Assert.Equal("EMPLOYEE_NOT_FOUND", ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailExists_ThrowsBusinessException()
    {
        var existing = TestDataFactory.CreateEmployee(1);
        var model = TestDataFactory.EditEmployeeModel(id: 1, email: "taken@example.com");

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _employeeRepositoryMock
            .Setup(r => r.EmailExistsAsync("taken@example.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _employeeService.UpdateAsync(model));

        Assert.Equal("EMPLOYEE_EMAIL_EXISTS", ex.ErrorCode);
        _employeeRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
