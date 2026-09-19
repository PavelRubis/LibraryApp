using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(202609190003)]
public sealed class CreateAuthorProcedures : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("Infrastructure.Migrations.Scripts.003_CreateAuthorProcedures.sql");
    }

    public override void Down()
    {
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Author_Search;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Author_GetById;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Author_SoftDelete;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Author_Update;");
        Execute.Sql("DROP PROCEDURE IF EXISTS library.Author_Insert;");
    }
}
