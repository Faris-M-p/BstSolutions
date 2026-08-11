namespace BstSolutions.Models;

/// <summary>
/// EF Core entity mapped to the Employees table.
/// SQL primary key column: ID_Employee (mapped via Fluent API).
/// </summary>
public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
}
