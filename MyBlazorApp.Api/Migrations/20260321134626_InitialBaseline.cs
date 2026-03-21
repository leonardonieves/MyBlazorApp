using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBlazorApp.Api.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Baseline migration - tables already exist in database.
    /// This migration is empty because the schema was created by the original MyBlazorApp project.
    /// </summary>
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty - tables already exist
            // This migration serves as a baseline for future migrations
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty - we don't want to drop existing tables
        }
    }
}
