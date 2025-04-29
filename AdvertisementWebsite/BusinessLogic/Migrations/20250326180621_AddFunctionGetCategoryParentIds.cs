using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionGetCategoryParentIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""" 
                                CREATE FUNCTION get_parent_category_ids (category_id int) 
                                RETURNS TABLE ("Id" int)
                                AS $$
                                WITH RECURSIVE parent_category_ids AS (
               	                    SELECT c."ParentCategoryId" from "Categories" c 
               	                    WHERE c."Id" = category_id AND c."ParentCategoryId" IS NOT NULL
               	                    UNION ALL
               	                    SELECT c."ParentCategoryId" FROM "Categories" c, parent_category_ids 
               	                    WHERE c."Id" = parent_category_ids."ParentCategoryId" AND c."ParentCategoryId" IS NOT NULL)

                                SELECT "ParentCategoryId" FROM parent_category_ids;
                                $$ LANGUAGE SQL;
               """");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION get_parent_category_ids (int) ");
        }
    }
}
