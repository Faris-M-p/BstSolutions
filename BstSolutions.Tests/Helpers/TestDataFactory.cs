using BstSolutions.Common.Enums;
using BstSolutions.Models;
using BstSolutions.ViewModels.Employee;
using BstSolutions.ViewModels.Task;

namespace BstSolutions.Tests.Helpers;

public static class TestDataFactory
{
    public static Employee CreateEmployee(
        int id = 1,
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        bool isActive = true)
    {
        return new Employee
        {
            ID_Employee = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static CreateEmployeeViewModel CreateEmployeeModel(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com")
    {
        return new CreateEmployeeViewModel
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };
    }

    public static EditEmployeeViewModel EditEmployeeModel(
        int id = 1,
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        bool isActive = true)
    {
        return new EditEmployeeViewModel
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = isActive
        };
    }

    public static WorkTask CreateTask(
        int id = 1,
        int employeeId = 1,
        string title = "Sample Task",
        WorkTaskStatus status = WorkTaskStatus.New,
        Priority priority = Priority.Medium)
    {
        return new WorkTask
        {
            ID_WorkTask = id,
            Title = title,
            Description = "Sample description",
            FK_Employee = employeeId,
            Priority = priority,
            Status = status,
            DueDate = DateTime.Today.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            CompletedDate = null,
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };
    }

    public static CreateTaskViewModel CreateTaskModel(
        int employeeId = 1,
        string title = "Sample Task",
        Priority priority = Priority.Medium)
    {
        return new CreateTaskViewModel
        {
            Title = title,
            Description = "Sample description",
            EmployeeId = employeeId,
            Priority = priority,
            DueDate = DateTime.Today.AddDays(7)
        };
    }

    public static EditTaskViewModel EditTaskModel(
        int id = 1,
        int employeeId = 1,
        string title = "Updated Task",
        WorkTaskStatus status = WorkTaskStatus.InProgress,
        Priority priority = Priority.High)
    {
        return new EditTaskViewModel
        {
            Id = id,
            Title = title,
            Description = "Updated description",
            EmployeeId = employeeId,
            Priority = priority,
            Status = status,
            DueDate = DateTime.Today.AddDays(3),
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };
    }
}
