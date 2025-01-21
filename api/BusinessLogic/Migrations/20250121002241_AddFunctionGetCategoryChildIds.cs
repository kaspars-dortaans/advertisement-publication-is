using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionGetCategoryChildIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""" 
                                CREATE FUNCTION get_child_category_ids (category_id int) 
                                RETURNS TABLE ("Id" int)
                                AS $$
                                WITH RECURSIVE child_category_ids AS (SELECT "Id" from "Categories" c WHERE c."ParentCategoryId" = category_id
                                                    UNION ALL
                                                    SELECT c."Id" FROM "Categories" c, child_category_ids WHERE c."ParentCategoryId" = child_category_ids."Id")
                                SELECT "Id" FROM child_category_ids;
                                $$ LANGUAGE SQL;
               """");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION get_child_category_ids (int) ");
        }
    }
}
