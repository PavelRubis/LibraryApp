using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(202609190001)]
public sealed class CreateLibrarySchema : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("Infrastructure.Migrations.Scripts.001_CreateSchema.sql");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS library.BookAuthors;");
        Execute.Sql("DROP TABLE IF EXISTS library.Books;");
        Execute.Sql("DROP TABLE IF EXISTS library.Authors;");
        Execute.Sql("DROP SCHEMA IF EXISTS library;");
    }
}
