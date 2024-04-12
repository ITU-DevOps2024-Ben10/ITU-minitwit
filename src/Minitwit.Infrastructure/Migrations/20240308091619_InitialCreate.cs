using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Minitwit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Cheeps",
                    columns: table => new
                    {
                        CheepId = table.Column<Guid>(type: "char(36)", nullable: false),
                        AuthorId = table.Column<Guid>(type: "char(36)", nullable: false),
                        Text = table.Column<string>(
                            type: "varchar(160)",
                            maxLength: 160,
                            nullable: false
                        ),
                        TimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Cheeps", x => x.CheepId);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Follows",
                    columns: table => new
                    {
                        FollowingAuthorId = table.Column<Guid>(type: "char(36)", nullable: false),
                        FollowedAuthorId = table.Column<Guid>(type: "char(36)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey(
                            "PK_Follows",
                            x => new { x.FollowingAuthorId, x.FollowedAuthorId }
                        );
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Reactions",
                    columns: table => new
                    {
                        CheepId = table.Column<Guid>(type: "char(36)", nullable: false),
                        AuthorId = table.Column<Guid>(type: "char(36)", nullable: false),
                        ReactionType = table.Column<string>(type: "longtext", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Reactions", x => new { x.CheepId, x.AuthorId });
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "RoleClaims",
                    columns: table => new
                    {
                        Id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySQL:ValueGenerationStrategy",
                                MySQLValueGenerationStrategy.IdentityColumn
                            ),
                        RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                        ClaimType = table.Column<string>(type: "longtext", nullable: true),
                        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Roles",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false),
                        Name = table.Column<string>(type: "longtext", nullable: true),
                        NormalizedName = table.Column<string>(type: "longtext", nullable: true),
                        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Roles", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "UserClaims",
                    columns: table => new
                    {
                        Id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySQL:ValueGenerationStrategy",
                                MySQLValueGenerationStrategy.IdentityColumn
                            ),
                        UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                        ClaimType = table.Column<string>(type: "longtext", nullable: true),
                        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_UserClaims", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "UserLogins",
                    columns: table => new
                    {
                        UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                        LoginProvider = table.Column<string>(type: "varchar(255)", nullable: true),
                        ProviderKey = table.Column<string>(type: "varchar(255)", nullable: true),
                        ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_UserLogins", x => x.UserId);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "UserRoles",
                    columns: table => new
                    {
                        RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                        UserId = table.Column<Guid>(type: "char(36)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_UserRoles", x => x.RoleId);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Users",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false),
                        UserName = table.Column<string>(
                            type: "varchar(50)",
                            maxLength: 50,
                            nullable: false
                        ),
                        Email = table.Column<string>(
                            type: "varchar(50)",
                            maxLength: 50,
                            nullable: false
                        ),
                        NormalizedUserName = table.Column<string>(type: "longtext", nullable: true),
                        NormalizedEmail = table.Column<string>(type: "longtext", nullable: true),
                        EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                        SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                        PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                        PhoneNumberConfirmed = table.Column<bool>(
                            type: "tinyint(1)",
                            nullable: false
                        ),
                        TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                        LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Users", x => x.Id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "UserTokens",
                    columns: table => new
                    {
                        UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                        LoginProvider = table.Column<string>(type: "longtext", nullable: true),
                        Name = table.Column<string>(type: "longtext", nullable: true),
                        Value = table.Column<string>(type: "longtext", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_UserTokens", x => x.UserId);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Cheeps_CheepId",
                table: "Cheeps",
                column: "CheepId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_LoginProvider_ProviderKey",
                table: "UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Cheeps");

            migrationBuilder.DropTable(name: "Follows");

            migrationBuilder.DropTable(name: "Reactions");

            migrationBuilder.DropTable(name: "RoleClaims");

            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.DropTable(name: "UserClaims");

            migrationBuilder.DropTable(name: "UserLogins");

            migrationBuilder.DropTable(name: "UserRoles");

            migrationBuilder.DropTable(name: "Users");

            migrationBuilder.DropTable(name: "UserTokens");
        }
    }
}
