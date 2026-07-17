using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProTableRateLine_UpdateJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with USING clause to cast text to jsonb
            migrationBuilder.Sql(
                "ALTER TABLE pro_table_rate_line ALTER COLUMN condition TYPE jsonb USING condition::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cast jsonb back to text
            migrationBuilder.Sql(
                "ALTER TABLE pro_table_rate_line ALTER COLUMN condition TYPE text USING condition::text;");
        }
    }
}
