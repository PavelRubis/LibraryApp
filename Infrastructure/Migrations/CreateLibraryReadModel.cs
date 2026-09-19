using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(202609190002)]
public sealed class CreateLibraryReadModel : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("Infrastructure.Migrations.Scripts.002_CreateReadModel.sql");
    }

    public override void Down()
    {
        Execute.Sql("DROP VIEW IF EXISTS library.ActiveBooks;");
        Execute.Sql("DROP VIEW IF EXISTS library.ActiveAuthors;");
        Execute.Sql("DROP TYPE IF EXISTS library.EntityIdList;");
    }
}
