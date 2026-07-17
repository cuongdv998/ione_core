using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class FixInsurerDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_insurerdictionary",
                table: "insurerdictionary");

            migrationBuilder.RenameTable(
                name: "insurerdictionary",
                newName: "insurer_dictionary");

            migrationBuilder.RenameColumn(
                name: "owncode",
                table: "insurer_dictionary",
                newName: "own_code");

            migrationBuilder.RenameColumn(
                name: "lastmodifierid",
                table: "insurer_dictionary",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "lastmodificationtime",
                table: "insurer_dictionary",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "insurercode",
                table: "insurer_dictionary",
                newName: "insurer_code");

            migrationBuilder.RenameColumn(
                name: "extradata",
                table: "insurer_dictionary",
                newName: "extra_data");

            migrationBuilder.RenameColumn(
                name: "expiredate",
                table: "insurer_dictionary",
                newName: "expire_date");

            migrationBuilder.RenameColumn(
                name: "effectdate",
                table: "insurer_dictionary",
                newName: "effect_date");

            migrationBuilder.RenameColumn(
                name: "creatorid",
                table: "insurer_dictionary",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "creationtime",
                table: "insurer_dictionary",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "businessname",
                table: "insurer_dictionary",
                newName: "business_name");

            migrationBuilder.AddColumn<Guid>(
                name: "insurer_id",
                table: "insurer_dictionary",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_insurer_dictionary",
                table: "insurer_dictionary",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_insurer_dictionary_business_insurer_own",
                table: "insurer_dictionary",
                columns: new[] { "business_name", "insurer_id", "own_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_insurer_dictionary",
                table: "insurer_dictionary");

            migrationBuilder.DropIndex(
                name: "ix_insurer_dictionary_business_insurer_own",
                table: "insurer_dictionary");

            migrationBuilder.DropColumn(
                name: "insurer_id",
                table: "insurer_dictionary");

            migrationBuilder.RenameTable(
                name: "insurer_dictionary",
                newName: "insurerdictionary");

            migrationBuilder.RenameColumn(
                name: "own_code",
                table: "insurerdictionary",
                newName: "owncode");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "insurerdictionary",
                newName: "lastmodifierid");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "insurerdictionary",
                newName: "lastmodificationtime");

            migrationBuilder.RenameColumn(
                name: "insurer_code",
                table: "insurerdictionary",
                newName: "insurercode");

            migrationBuilder.RenameColumn(
                name: "extra_data",
                table: "insurerdictionary",
                newName: "extradata");

            migrationBuilder.RenameColumn(
                name: "expire_date",
                table: "insurerdictionary",
                newName: "expiredate");

            migrationBuilder.RenameColumn(
                name: "effect_date",
                table: "insurerdictionary",
                newName: "effectdate");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "insurerdictionary",
                newName: "creatorid");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "insurerdictionary",
                newName: "creationtime");

            migrationBuilder.RenameColumn(
                name: "business_name",
                table: "insurerdictionary",
                newName: "businessname");

            migrationBuilder.AddPrimaryKey(
                name: "PK_insurerdictionary",
                table: "insurerdictionary",
                column: "id");
        }
    }
}
