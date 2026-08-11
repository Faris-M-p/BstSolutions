/*
    Indexes - master definitions for common filter fields.
    Keep indexes simple and purposeful.
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WorkTasks_FK_Employee'
      AND object_id = OBJECT_ID(N'dbo.WorkTasks')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkTasks_FK_Employee
        ON dbo.WorkTasks (FK_Employee);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WorkTasks_Status'
      AND object_id = OBJECT_ID(N'dbo.WorkTasks')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkTasks_Status
        ON dbo.WorkTasks (Status);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WorkTasks_Priority'
      AND object_id = OBJECT_ID(N'dbo.WorkTasks')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkTasks_Priority
        ON dbo.WorkTasks (Priority);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WorkTasks_DueDate'
      AND object_id = OBJECT_ID(N'dbo.WorkTasks')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkTasks_DueDate
        ON dbo.WorkTasks (DueDate);
END
GO
