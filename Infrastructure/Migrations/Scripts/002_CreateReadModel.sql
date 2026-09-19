CREATE TYPE library.EntityIdList AS TABLE
(
    Id uniqueidentifier NOT NULL PRIMARY KEY
);
GO

CREATE OR ALTER VIEW library.ActiveAuthors
AS
    SELECT Id, Name, CreatedAt, UpdatedAt, RowVersion
    FROM library.Authors
    WHERE IsDeleted = 0;
GO

CREATE OR ALTER VIEW library.ActiveBooks
AS
    SELECT Id, Title, PublicationYear, TableOfContents, CreatedAt, UpdatedAt, RowVersion
    FROM library.Books
    WHERE IsDeleted = 0;
GO

