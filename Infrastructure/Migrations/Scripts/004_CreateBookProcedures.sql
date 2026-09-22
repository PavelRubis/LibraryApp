CREATE OR ALTER PROCEDURE library.Book_Insert
    @Id uniqueidentifier,
    @Title nvarchar(300),
    @PublicationYear smallint,
    @TableOfContents xml,
    @AuthorIds library.EntityIdList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM @AuthorIds)
       OR EXISTS (
            SELECT 1
            FROM @AuthorIds ids
            LEFT JOIN library.Authors a WITH (UPDLOCK, HOLDLOCK)
                ON a.Id = ids.Id AND a.IsDeleted = 0
            WHERE a.Id IS NULL)
    BEGIN
        SELECT 3;
        RETURN;
    END;

    INSERT INTO library.Books (Id, Title, PublicationYear, TableOfContents)
    VALUES (@Id, @Title, @PublicationYear, @TableOfContents);

    INSERT INTO library.BookAuthors (BookId, AuthorId)
    SELECT @Id, Id FROM @AuthorIds;

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Book_Update
    @Id uniqueidentifier,
    @Title nvarchar(300),
    @PublicationYear smallint,
    @TableOfContents xml,
    @AuthorIds library.EntityIdList READONLY,
    @ExpectedRowVersion binary(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM @AuthorIds)
       OR EXISTS (
            SELECT 1
            FROM @AuthorIds ids
            LEFT JOIN library.Authors a WITH (UPDLOCK, HOLDLOCK)
                ON a.Id = ids.Id AND a.IsDeleted = 0
            WHERE a.Id IS NULL)
    BEGIN
        SELECT 3;
        RETURN;
    END;

    UPDATE library.Books
    SET Title = @Title,
        PublicationYear = @PublicationYear,
        TableOfContents = @TableOfContents,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id
      AND IsDeleted = 0
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM library.Books WHERE Id = @Id AND IsDeleted = 0)
            SELECT 2;
        ELSE
            SELECT 1;
        RETURN;
    END;

    DELETE FROM library.BookAuthors WHERE BookId = @Id;

    INSERT INTO library.BookAuthors (BookId, AuthorId)
    SELECT @Id, Id FROM @AuthorIds;

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Book_SoftDelete
    @Id uniqueidentifier,
    @ExpectedRowVersion binary(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE library.Books
    SET IsDeleted = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id
      AND IsDeleted = 0
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM library.Books WHERE Id = @Id AND IsDeleted = 0)
            SELECT 2;
        ELSE
            SELECT 1;
        RETURN;
    END;

    SELECT 0;
END;
GO

CREATE OR ALTER PROCEDURE library.Book_GetById
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id,
           Title,
           PublicationYear,
           CONVERT(nvarchar(max), TableOfContents) AS TableOfContentsXml,
           CreatedAt,
           UpdatedAt,
           RowVersion
    FROM library.ActiveBooks
    WHERE Id = @Id;

    SELECT a.Id,
           a.Name,
           a.CreatedAt,
           a.UpdatedAt,
           a.RowVersion
    FROM library.BookAuthors ba
    INNER JOIN library.ActiveBooks b ON b.Id = ba.BookId
    INNER JOIN library.ActiveAuthors a ON a.Id = ba.AuthorId
    WHERE ba.BookId = @Id
    ORDER BY a.Name, a.Id;
END;
GO

CREATE OR ALTER PROCEDURE library.Book_Search
    @Query nvarchar(300) = NULL,
    @SearchFields int = 7,
    @Page int = 1,
    @PageSize int = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 1;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Query = NULLIF(LTRIM(RTRIM(@Query)), N'');
    DECLARE @Pattern nvarchar(606) = CASE WHEN @Query IS NULL THEN NULL ELSE
        N'%' + REPLACE(REPLACE(REPLACE(@Query, N'~', N'~~'), N'%', N'~%'), N'_', N'~_') + N'%' END;

    CREATE TABLE #PagedBooks
    (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        SortOrder int NOT NULL
    );

    DECLARE @TotalCount int;

    SELECT @TotalCount = COUNT(*)
    FROM library.ActiveBooks b
    WHERE @Pattern IS NULL
       OR ((@SearchFields & 1) = 1 AND b.Title LIKE @Pattern ESCAPE N'~')
       OR ((@SearchFields & 4) = 4 AND CONVERT(nvarchar(max), b.TableOfContents) COLLATE Cyrillic_General_100_CI_AI LIKE @Pattern ESCAPE N'~')
       OR ((@SearchFields & 2) = 2 AND EXISTS (
            SELECT 1
            FROM library.BookAuthors ba
            INNER JOIN library.ActiveAuthors a ON a.Id = ba.AuthorId
            WHERE ba.BookId = b.Id
              AND a.Name LIKE @Pattern ESCAPE N'~'));

    ;WITH Candidates AS
    (
        SELECT b.Id,
               ROW_NUMBER() OVER (ORDER BY b.Title, b.Id) AS SortOrder
        FROM library.ActiveBooks b
        WHERE @Pattern IS NULL
           OR ((@SearchFields & 1) = 1 AND b.Title LIKE @Pattern ESCAPE N'~')
           OR ((@SearchFields & 4) = 4 AND CONVERT(nvarchar(max), b.TableOfContents) COLLATE Cyrillic_General_100_CI_AI LIKE @Pattern ESCAPE N'~')
           OR ((@SearchFields & 2) = 2 AND EXISTS (
                SELECT 1
                FROM library.BookAuthors ba
                INNER JOIN library.ActiveAuthors a ON a.Id = ba.AuthorId
                WHERE ba.BookId = b.Id
                  AND a.Name LIKE @Pattern ESCAPE N'~'))
    )
    INSERT INTO #PagedBooks (Id, SortOrder)
    SELECT Id, SortOrder
    FROM Candidates
    WHERE SortOrder BETWEEN ((@Page - 1) * @PageSize) + 1 AND @Page * @PageSize;

    SELECT b.Id,
           b.Title,
           b.PublicationYear,
           CONVERT(nvarchar(max), b.TableOfContents) AS TableOfContentsXml,
           b.CreatedAt,
           b.UpdatedAt,
           b.RowVersion
    FROM #PagedBooks p
    INNER JOIN library.ActiveBooks b ON b.Id = p.Id
    ORDER BY p.SortOrder;

    SELECT p.Id AS BookId,
           a.Id,
           a.Name,
           a.CreatedAt,
           a.UpdatedAt,
           a.RowVersion
    FROM #PagedBooks p
    INNER JOIN library.BookAuthors ba ON ba.BookId = p.Id
    INNER JOIN library.ActiveAuthors a ON a.Id = ba.AuthorId
    ORDER BY p.SortOrder, a.Name, a.Id;

    SELECT @TotalCount;
END;
GO
