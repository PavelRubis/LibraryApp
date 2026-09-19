using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(202609190004)]
public sealed class CreateBookProcedures : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("Infrastructure.Migrations.Scripts.004_CreateBookProcedures.sql");
    }

    public override void Down()
    {
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Book_Search;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Book_GetById;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Book_SoftDelete;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Book_Update;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Book_Insert;");
    }
}
