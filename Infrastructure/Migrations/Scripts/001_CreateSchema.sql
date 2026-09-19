IF SCHEMA_ID(N'library') IS NULL
    EXEC(N'CREATE SCHEMA library');

CREATE TABLE library.Authors
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Authors PRIMARY KEY,
    Name nvarchar(200) COLLATE Cyrillic_General_100_CI_AI NOT NULL,
    CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Authors_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2(7) NULL,
    IsDeleted bit NOT NULL CONSTRAINT DF_Authors_IsDeleted DEFAULT 0,
    RowVersion rowversion NOT NULL
);

CREATE TABLE library.Books
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
    Title nvarchar(300) COLLATE Cyrillic_General_100_CI_AI NOT NULL,
    PublicationYear smallint NOT NULL,
    TableOfContents xml NOT NULL,
    CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Books_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2(7) NULL,
    IsDeleted bit NOT NULL CONSTRAINT DF_Books_IsDeleted DEFAULT 0,
    RowVersion rowversion NOT NULL,
    CONSTRAINT CK_Books_PublicationYear CHECK (PublicationYear BETWEEN 1 AND 9999)
);

CREATE TABLE library.BookAuthors
(
    BookId uniqueidentifier NOT NULL,
    AuthorId uniqueidentifier NOT NULL,
    CONSTRAINT PK_BookAuthors PRIMARY KEY (BookId, AuthorId),
    CONSTRAINT FK_BookAuthors_Books FOREIGN KEY (BookId) REFERENCES library.Books(Id),
    CONSTRAINT FK_BookAuthors_Authors FOREIGN KEY (AuthorId) REFERENCES library.Authors(Id)
);

CREATE UNIQUE INDEX UX_Authors_Active_Name
    ON library.Authors(Name)
    WHERE IsDeleted = 0;

CREATE INDEX IX_Authors_Active_Name
    ON library.Authors(Name, Id)
    INCLUDE (CreatedAt, UpdatedAt)
    WHERE IsDeleted = 0;

CREATE INDEX IX_Books_Active_Title
    ON library.Books(Title, Id)
    INCLUDE (PublicationYear, CreatedAt, UpdatedAt)
    WHERE IsDeleted = 0;

CREATE INDEX IX_BookAuthors_AuthorId_BookId
    ON library.BookAuthors(AuthorId, BookId);

