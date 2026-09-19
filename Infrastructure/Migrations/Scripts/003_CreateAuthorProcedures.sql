CREATE OR ALTER PROCEDURE library.Author_Insert
    @Id uniqueidentifier,
    @Name nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (
        SELECT 1
        FROM library.Authors WITH (UPDLOCK, HOLDLOCK)
        WHERE Name = @Name AND IsDeleted = 0)
    BEGIN
        SELECT 2;
        RETURN;
    END;

    INSERT INTO library.Authors (Id, Name)
    VALUES (@Id, @Name);

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Author_Update
    @Id uniqueidentifier,
    @Name nvarchar(200),
    @ExpectedRowVersion binary(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (
        SELECT 1
        FROM library.Authors WITH (UPDLOCK, HOLDLOCK)
        WHERE Name = @Name AND Id <> @Id AND IsDeleted = 0)
    BEGIN
        SELECT 2;
        RETURN;
    END;

    UPDATE library.Authors
    SET Name = @Name,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id
      AND IsDeleted = 0
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM library.Authors WHERE Id = @Id AND IsDeleted = 0)
            SELECT 2;
        ELSE
            SELECT 1;
        RETURN;
    END;

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Author_SoftDelete
    @Id uniqueidentifier,
    @ExpectedRowVersion binary(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM library.Authors WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @Id AND IsDeleted = 0)
    BEGIN
        SELECT 1;
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM library.Authors
        WHERE Id = @Id
          AND IsDeleted = 0
          AND RowVersion = @ExpectedRowVersion)
    BEGIN
        SELECT 2;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM library.BookAuthors ba
        INNER JOIN library.ActiveBooks b ON b.Id = ba.BookId
        WHERE ba.AuthorId = @Id)
    BEGIN
        SELECT 2;
        RETURN;
    END;

    UPDATE library.Authors
    SET IsDeleted = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id
      AND IsDeleted = 0
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT 2;
        RETURN;
    END;

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Author_GetById
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, CreatedAt, UpdatedAt, RowVersion
    FROM library.ActiveAuthors
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE library.Author_Search
    @Query nvarchar(200) = NULL,
    @Page int = 1,
    @PageSize int = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 1;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Query = NULLIF(LTRIM(RTRIM(@Query)), N'');
    DECLARE @Pattern nvarchar(406) = CASE WHEN @Query IS NULL THEN NULL ELSE
        N'%' + REPLACE(REPLACE(REPLACE(@Query, N'~', N'~~'), N'%', N'~%'), N'_', N'~_') + N'%' END;

    SELECT Id,
           Name,
           CreatedAt,
           UpdatedAt,
           RowVersion
    FROM library.ActiveAuthors
    WHERE @Pattern IS NULL OR Name LIKE @Pattern ESCAPE N'~'
    ORDER BY Name, Id
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM library.ActiveAuthors
    WHERE @Pattern IS NULL OR Name LIKE @Pattern ESCAPE N'~';
END;
GO
