using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PartyMemberManager.Dal.Migrations
{
    public partial class init0001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    SchoolAreas = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LogLevel = table.Column<string>(maxLength: 50, nullable: true),
                    CategoryName = table.Column<string>(maxLength: 500, nullable: true),
                    Message = table.Column<string>(maxLength: 5000, nullable: true),
                    User = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Area = table.Column<string>(nullable: true),
                    Controller = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    Roles = table.Column<int>(nullable: false),
                    ParentModuleId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Modules_ParentModuleId",
                        column: x => x.ParentModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Nations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartySchools",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartySchools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainClassTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainClassTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveApplicationSurveies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Year = table.Column<string>(maxLength: 4, nullable: false),
                    SchoolArea = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: true),
                    Term = table.Column<int>(nullable: false),
                    Total = table.Column<int>(nullable: false),
                    TrainTotal = table.Column<int>(nullable: false),
                    Proportion = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveApplicationSurveies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveApplicationSurveies_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CadreTrains",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Year = table.Column<string>(maxLength: 4, nullable: false),
                    TrainClass = table.Column<string>(maxLength: 50, nullable: false),
                    Organizer = table.Column<string>(maxLength: 50, nullable: false),
                    TrainOrganizational = table.Column<string>(maxLength: 50, nullable: false),
                    TrainTime = table.Column<DateTime>(nullable: false),
                    TrainDuration = table.Column<string>(maxLength: 10, nullable: false),
                    CadreTrainType = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 20, nullable: false),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    IDNumber = table.Column<string>(maxLength: 20, nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: false),
                    Post = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadreTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CadreTrains_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LoginName = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 20, nullable: false),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    Password = table.Column<string>(maxLength: 100, nullable: false),
                    Roles = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operators_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyActivists",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    StudentNo = table.Column<string>(maxLength: 20, nullable: false),
                    IDNumber = table.Column<string>(maxLength: 20, nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    BirthDate = table.Column<string>(maxLength: 20, nullable: false),
                    Nationality = table.Column<string>(maxLength: 10, nullable: false),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    DepartmentId = table.Column<Guid>(nullable: true),
                    Class = table.Column<string>(nullable: true),
                    ApplicationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyActivists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyActivists_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PotentialMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    StudentNo = table.Column<string>(maxLength: 20, nullable: false),
                    IDNumber = table.Column<string>(maxLength: 20, nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    DepartmentId = table.Column<Guid>(nullable: true),
                    ApplicationTime = table.Column<DateTime>(nullable: false),
                    ActiveApplicationTime = table.Column<DateTime>(nullable: false),
                    PotentialMemberTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PotentialMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PotentialMembers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Code = table.Column<string>(maxLength: 2, nullable: false),
                    TrainClassTypeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainClasses_TrainClassTypes_TrainClassTypeId",
                        column: x => x.TrainClassTypeId,
                        principalTable: "TrainClassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatorModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ModuleId = table.Column<Guid>(nullable: false),
                    RightType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperatorModules_Operators_UserId",
                        column: x => x.UserId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    OperatorId = table.Column<Guid>(nullable: true),
                    Ordinal = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Year = table.Column<string>(maxLength: 4, nullable: false),
                    TrainClassTypeId = table.Column<Guid>(nullable: true),
                    PartyActivistId = table.Column<Guid>(nullable: false),
                    PSGrade = table.Column<double>(nullable: false),
                    CSGrade = table.Column<double>(nullable: false),
                    TotalGrade = table.Column<double>(nullable: false),
                    IsPass = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainResults_PartyActivists_PartyActivistId",
                        column: x => x.PartyActivistId,
                        principalTable: "PartyActivists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainResults_TrainClassTypes_TrainClassTypeId",
                        column: x => x.TrainClassTypeId,
                        principalTable: "TrainClassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveApplicationSurveies_DepartmentId",
                table: "ActiveApplicationSurveies",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CadreTrains_DepartmentId",
                table: "CadreTrains",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ParentModuleId",
                table: "Modules",
                column: "ParentModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorModules_ModuleId",
                table: "OperatorModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorModules_UserId",
                table: "OperatorModules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_DepartmentId",
                table: "Operators",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_LoginName",
                table: "Operators",
                column: "LoginName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyActivists_DepartmentId",
                table: "PartyActivists",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialMembers_DepartmentId",
                table: "PotentialMembers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainClasses_TrainClassTypeId",
                table: "TrainClasses",
                column: "TrainClassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainResults_PartyActivistId",
                table: "TrainResults",
                column: "PartyActivistId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainResults_TrainClassTypeId",
                table: "TrainResults",
                column: "TrainClassTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveApplicationSurveies");

            migrationBuilder.DropTable(
                name: "CadreTrains");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Nations");

            migrationBuilder.DropTable(
                name: "OperatorModules");

            migrationBuilder.DropTable(
                name: "PartySchools");

            migrationBuilder.DropTable(
                name: "PotentialMembers");

            migrationBuilder.DropTable(
                name: "TrainClasses");

            migrationBuilder.DropTable(
                name: "TrainResults");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "PartyActivists");

            migrationBuilder.DropTable(
                name: "TrainClassTypes");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
