using BstSolutions.Common;
using BstSolutions.Common.Enums;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services;
using BstSolutions.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BstSolutions.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _employeeRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeIsActive_AddsTaskAndSaves()
    {
        var employee = TestDataFactory.CreateEmployee(1, isActive: true);
        var model = TestDataFactory.CreateTaskModel(employeeId: 1, title: " New Task ");

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        await _taskService.CreateAsync(model);

        _taskRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<WorkTask>(t =>
                    t.Title == "New Task" &&
                    t.FK_Employee == 1 &&
                    t.Status == WorkTaskStatus.New &&
                    t.CompletedDate == null),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeMissing_ThrowsNotFoundException()
    {
        var model = TestDataFactory.CreateTaskModel(employeeId: 99);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _taskService.CreateAsync(model));

        Assert.Equal("TASK_EMPLOYEE_NOT_FOUND", ex.ErrorCode);
        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<WorkTask>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeInactive_ThrowsBusinessException()
    {
        var employee = TestDataFactory.CreateEmployee(1, isActive: false);
        var model = TestDataFactory.CreateTaskModel(employeeId: 1);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _taskService.CreateAsync(model));

        Assert.Equal("TASK_EMPLOYEE_INACTIVE", ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesTaskAndSaves()
    {
        var task = TestDataFactory.CreateTask(id: 1, employeeId: 1, status: WorkTaskStatus.New);
        var employee = TestDataFactory.CreateEmployee(1, isActive: true);
        var model = TestDataFactory.EditTaskModel(id: 1, employeeId: 1, title: " Updated ");

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        await _taskService.UpdateAsync(model);

        Assert.Equal("Updated", task.Title);
        Assert.Equal(WorkTaskStatus.InProgress, task.Status);

        _taskRepositoryMock.Verify(
            r => r.SetOriginalRowVersion(task, model.RowVersion),
            Times.Once);
        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskMissing_ThrowsNotFoundException()
    {
        var model = TestDataFactory.EditTaskModel(id: 99);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTask?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _taskService.UpdateAsync(model));

        Assert.Equal("TASK_NOT_FOUND", ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenReassigningToInactiveEmployee_ThrowsBusinessException()
    {
        var task = TestDataFactory.CreateTask(id: 1, employeeId: 1);
        var inactiveEmployee = TestDataFactory.CreateEmployee(2, isActive: false);
        var model = TestDataFactory.EditTaskModel(id: 1, employeeId: 2);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveEmployee);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _taskService.UpdateAsync(model));

        Assert.Equal("TASK_EMPLOYEE_INACTIVE", ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenConcurrencyConflict_ThrowsConflictException()
    {
        var task = TestDataFactory.CreateTask(id: 1, employeeId: 1);
        var employee = TestDataFactory.CreateEmployee(1, isActive: true);
        var model = TestDataFactory.EditTaskModel(id: 1, employeeId: 1);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _taskRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _taskService.UpdateAsync(model));

        Assert.Equal("CONCURRENCY_CONFLICT", ex.ErrorCode);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskIsNew_MarksCompletedAndSaves()
    {
        var task = TestDataFactory.CreateTask(id: 1, status: WorkTaskStatus.New);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        await _taskService.CompleteAsync(1);

        Assert.Equal(WorkTaskStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedDate);
        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskMissing_ThrowsNotFoundException()
    {
        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTask?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _taskService.CompleteAsync(99));

        Assert.Equal("TASK_NOT_FOUND", ex.ErrorCode);
    }

    [Fact]
    public async Task CompleteAsync_WhenCancelled_ThrowsBusinessException()
    {
        var task = TestDataFactory.CreateTask(id: 1, status: WorkTaskStatus.Cancelled);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _taskService.CompleteAsync(1));

        Assert.Equal("TASK_INVALID_STATUS", ex.ErrorCode);
    }

    [Fact]
    public async Task CompleteAsync_WhenAlreadyCompleted_ThrowsBusinessException()
    {
        var task = TestDataFactory.CreateTask(id: 1, status: WorkTaskStatus.Completed);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _taskService.CompleteAsync(1));

        Assert.Equal("TASK_ALREADY_COMPLETED", ex.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskNotCompleted_DeletesAndSaves()
    {
        var task = TestDataFactory.CreateTask(id: 1, status: WorkTaskStatus.New);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        await _taskService.DeleteAsync(1);

        _taskRepositoryMock.Verify(
            r => r.DeleteAsync(task, It.IsAny<CancellationToken>()),
            Times.Once);
        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCompleted_ThrowsBusinessException()
    {
        var task = TestDataFactory.CreateTask(id: 1, status: WorkTaskStatus.Completed);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _taskService.DeleteAsync(1));

        Assert.Equal("TASK_COMPLETED_CANNOT_DELETE", ex.ErrorCode);
        _taskRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<WorkTask>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskMissing_ThrowsNotFoundException()
    {
        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTask?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _taskService.DeleteAsync(99));

        Assert.Equal("TASK_NOT_FOUND", ex.ErrorCode);
    }
}
