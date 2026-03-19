using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaasACC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSistemaAprobacionClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AprobadoPorUsuarioId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoId",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigenRegistro",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstadosCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosCliente", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EstadosCliente",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente de aprobación", "Pendiente" },
                    { 2, "Cliente activo", "Activo" },
                    { 3, "Cliente inactivo", "Inactivo" }
                });

            // Migrar datos existentes: todos los clientes actuales se marcan como Activos (EstadoId=2)
            // y con origen Autogestión (OrigenRegistro=2)
            migrationBuilder.Sql(@"
                UPDATE Clientes
                SET EstadoId = 2,
                    OrigenRegistro = 2
                WHERE EstadoId = 0
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_AprobadoPorUsuarioId",
                table: "Clientes",
                column: "AprobadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EstadoId",
                table: "Clientes",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_EstadosCliente_EstadoId",
                table: "Clientes",
                column: "EstadoId",
                principalTable: "EstadosCliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_AprobadoPorUsuarioId",
                table: "Clientes",
                column: "AprobadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_EstadosCliente_EstadoId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_AprobadoPorUsuarioId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosCliente");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_AprobadoPorUsuarioId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_EstadoId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AprobadoPorUsuarioId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EstadoId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "OrigenRegistro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Clientes");
        }
    }
}
