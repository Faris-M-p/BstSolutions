/*
    ApplicationUsers - master table definition (latest complete schema).

    Naming rules:
    - Primary key: ID_ApplicationUser

    Authentication users only. Not related to Employees.
*/

IF OBJECT_ID(N'dbo.ApplicationUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationUsers
    (
        ID_ApplicationUser  INT             NOT NULL IDENTITY(1, 1),
        Email               NVARCHAR(256)   NOT NULL,
        PasswordHash        NVARCHAR(500)   NOT NULL,
        IsActive            BIT             NOT NULL CONSTRAINT DF_ApplicationUsers_IsActive DEFAULT (1),
        Role                NVARCHAR(50)    NOT NULL,
        CreatedDate         DATETIME2(7)    NOT NULL CONSTRAINT DF_ApplicationUsers_CreatedDate DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ApplicationUsers PRIMARY KEY CLUSTERED (ID_ApplicationUser),
        CONSTRAINT UQ_ApplicationUsers_Email UNIQUE (Email)
    );
END
GO
