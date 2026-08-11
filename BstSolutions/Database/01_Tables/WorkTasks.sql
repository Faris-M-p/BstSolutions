/*
    WorkTasks - master table definition (latest complete schema).

    Naming rules:
    - Primary key: ID_WorkTask
    - Foreign key column: FK_Employee → Employees.ID_Employee

    Priority INT values (match BstSolutions.Common.Enums.Priority):
        1 = Low, 2 = Medium, 3 = High, 4 = Critical

    Status INT values (match BstSolutions.Common.Enums.WorkTaskStatus):
        1 = New, 2 = InProgress, 3 = Completed, 4 = Cancelled
*/

IF OBJECT_ID(N'dbo.WorkTasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkTasks
    (
        ID_WorkTask    INT             NOT NULL IDENTITY(1, 1),
        Title          NVARCHAR(150)   NOT NULL,
        Description    NVARCHAR(2000)  NULL,
        FK_Employee    INT             NOT NULL,
        Priority       INT             NOT NULL,
        Status         INT             NOT NULL,
        DueDate        DATE            NOT NULL,
        CreatedDate    DATETIME2(7)    NOT NULL CONSTRAINT DF_WorkTasks_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CompletedDate  DATETIME2(7)    NULL,

        CONSTRAINT PK_WorkTasks PRIMARY KEY CLUSTERED (ID_WorkTask),
        CONSTRAINT FK_WorkTasks_Employee
            FOREIGN KEY (FK_Employee) REFERENCES dbo.Employees (ID_Employee),
        CONSTRAINT CK_WorkTasks_Priority CHECK (Priority IN (1, 2, 3, 4)),
        CONSTRAINT CK_WorkTasks_Status CHECK (Status IN (1, 2, 3, 4))
    );
END
GO
