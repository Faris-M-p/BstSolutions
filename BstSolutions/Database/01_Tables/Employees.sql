/*
    Employees - master table definition (latest complete schema).

    Naming rules:
    - Primary key: ID_Employee
*/

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        ID_Employee  INT            NOT NULL IDENTITY(1, 1),
        FirstName    NVARCHAR(100)  NOT NULL,
        LastName     NVARCHAR(100)  NOT NULL,
        Email        NVARCHAR(256)  NOT NULL,
        IsActive     BIT            NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        CreatedDate  DATETIME2(7)   NOT NULL CONSTRAINT DF_Employees_CreatedDate DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (ID_Employee),
        CONSTRAINT UQ_Employees_Email UNIQUE (Email)
    );
END
GO
