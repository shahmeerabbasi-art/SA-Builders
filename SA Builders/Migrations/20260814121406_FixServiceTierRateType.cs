using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SA_Builders.Migrations
{
    /// <inheritdoc />
    public partial class FixServiceTierRateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BaseRatePerSqFt",
                table: "ServiceTiers",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "BaseRatePerSqFt",
                table: "ServiceTiers",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
