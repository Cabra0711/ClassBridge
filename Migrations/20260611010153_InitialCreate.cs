using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IUE.DesatrasadorMVP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaClase = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clases_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Excusas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    ClaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Excusas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Excusas_Clases_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "Clases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Excusas_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoClases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubidoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoClases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoClases_Clases_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "Clases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Estudiantes",
                columns: new[] { "Id", "Correo", "Nombre" },
                values: new object[,]
                {
                    { 1, "juan.perez@iue.edu.co", "Juan Pérez" },
                    { 2, "maria.lopez@iue.edu.co", "María López" }
                });

            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Fundamentos de programación", "Programación I" },
                    { 2, "SQL y modelado relacional", "Bases de Datos" },
                    { 3, "Límites, derivadas e integrales", "Cálculo Diferencial" }
                });

            migrationBuilder.InsertData(
                table: "Clases",
                columns: new[] { "Id", "FechaClase", "MateriaId", "Resumen", "Titulo" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Variables, tipos y control de flujo.", "Introducción a C#" },
                    { 2, new DateTime(2025, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Clases, objetos y herencia.", "POO en C#" },
                    { 3, new DateTime(2025, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "1FN, 2FN y 3FN explicadas.", "Normalización" },
                    { 4, new DateTime(2025, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Definición epsilon-delta.", "Límites y continuidad" }
                });

            migrationBuilder.InsertData(
                table: "Inscripciones",
                columns: new[] { "Id", "EstudianteId", "MateriaId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 2, 1 },
                    { 4, 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "Excusas",
                columns: new[] { "Id", "ClaseId", "Descripcion", "Estado", "EstudianteId", "FechaEnvio" },
                values: new object[,]
                {
                    { 1, 1, "Incapacidad médica", 1, 1, new DateTime(2025, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, "Problema familiar", 0, 1, new DateTime(2025, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, "Viaje académico", 1, 2, new DateTime(2025, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "VideoClases",
                columns: new[] { "Id", "ClaseId", "SubidoEn", "VideoUrl" },
                values: new object[] { 1, 1, new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://www.youtube.com/watch?v=dQw4w9WgXcQ" });

            migrationBuilder.CreateIndex(
                name: "IX_Clases_MateriaId",
                table: "Clases",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Excusas_ClaseId",
                table: "Excusas",
                column: "ClaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Excusas_EstudianteId",
                table: "Excusas",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_EstudianteId",
                table: "Inscripciones",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_MateriaId",
                table: "Inscripciones",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoClases_ClaseId",
                table: "VideoClases",
                column: "ClaseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Excusas");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "VideoClases");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Clases");

            migrationBuilder.DropTable(
                name: "Materias");
        }
    }
}
