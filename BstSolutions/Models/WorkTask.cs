using BstSolutions.Common.Enums;

namespace BstSolutions.Models;

public class WorkTask
{
    public int ID_WorkTask { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int FK_Employee { get; set; }

    public Priority Priority { get; set; }

    public WorkTaskStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Employee Employee { get; set; } = null!;
}
