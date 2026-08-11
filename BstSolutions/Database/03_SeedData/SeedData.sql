/*
    Seed data - master file for a brand-new database.

    Employees:
    - No automatic employee seed.
    - Create employees through Employee → Create.

    ApplicationUsers:
    - Seeds the technical-test/demo admin account.
    - PasswordHash was generated with ASP.NET Core PasswordHasher<T>
      for password "Admin@123" (plain text is NEVER stored).
    - Idempotent: inserts only if email does not already exist.
*/

IF NOT EXISTS (SELECT 1 FROM dbo.ApplicationUsers WHERE Email = N'admin@gmail.com')
BEGIN
    INSERT INTO dbo.ApplicationUsers
    (
        Email,
        PasswordHash,
        IsActive,
        Role,
        CreatedDate
    )
    VALUES
    (
        N'admin@gmail.com',
        -- PasswordHasher hash for: Admin@123 (technical-test/demo only)
        N'AQAAAAIAAYagAAAAEDHY3YuVo+R5SJKKIcCoz9p/a6PtXH1tvr77jH+rUnZ7AE/LzQe/MHicAuA9Cmjqgg==',
        1,
        N'Admin',
        SYSUTCDATETIME()
    );
END
GO
