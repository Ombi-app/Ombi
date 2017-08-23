using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class Inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "ApplicationConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuditArea = table.Column<int>(nullable: false),
                    AuditType = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true),
                    SettingsName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Agent = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    NotificationType = table.Column<int>(nullable: false),
                    Subject = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Alias = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    UserType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    ReleaseYear = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    GrandparentKey = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    ParentKey = table.Column<string>(nullable: true),
                    SeasonNumber = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexEpisode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadarrCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TheMovieDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadarrCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TvRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImdbId = table.Column<string>(nullable: true),
                    Overview = table.Column<string>(nullable: true),
                    PosterPath = table.Column<string>(nullable: true),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    RootFolder = table.Column<int>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TvDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Approved = table.Column<bool>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    Denied = table.Column<bool>(nullable: true),
                    DeniedReason = table.Column<string>(nullable: true),
                    ImdbId = table.Column<string>(nullable: true),
                    IssueId = table.Column<int>(nullable: true),
                    Overview = table.Column<string>(nullable: true),
                    PosterPath = table.Column<string>(nullable: true),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    RequestType = table.Column<int>(nullable: false),
                    RequestedDate = table.Column<DateTime>(nullable: false),
                    RequestedUserId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    TheMovieDbId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieRequests_AspNetUsers_RequestedUserId",
                        column: x => x.RequestedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexSeasonsContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentKey = table.Column<int>(nullable: false),
                    PlexContentId = table.Column<int>(nullable: false),
                    SeasonKey = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexSeasonsContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexSeasonsContent_PlexContent_PlexContentId",
                        column: x => x.PlexContentId,
                        principalTable: "PlexContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChildRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Approved = table.Column<bool>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    Denied = table.Column<bool>(nullable: true),
                    DeniedReason = table.Column<string>(nullable: true),
                    IssueId = table.Column<int>(nullable: true),
                    ParentRequestId = table.Column<int>(nullable: false),
                    RequestType = table.Column<int>(nullable: false),
                    RequestedDate = table.Column<DateTime>(nullable: false),
                    RequestedUserId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildRequests_TvRequests_ParentRequestId",
                        column: x => x.ParentRequestId,
                        principalTable: "TvRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildRequests_AspNetUsers_RequestedUserId",
                        column: x => x.RequestedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovieIssues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    IssueId = table.Column<int>(nullable: true),
                    MovieId = table.Column<int>(nullable: false),
                    Subect = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieIssues_MovieRequests_IssueId",
                        column: x => x.IssueId,
                        principalTable: "MovieRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovieIssues_MovieRequests_MovieId",
                        column: x => x.MovieId,
                        principalTable: "MovieRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvIssues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    IssueId = table.Column<int>(nullable: true),
                    Subect = table.Column<string>(nullable: true),
                    TvId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TvIssues_ChildRequests_IssueId",
                        column: x => x.IssueId,
                        principalTable: "ChildRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TvIssues_ChildRequests_TvId",
                        column: x => x.TvId,
                        principalTable: "ChildRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildRequestId = table.Column<int>(nullable: false),
                    SeasonNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonRequests_ChildRequests_ChildRequestId",
                        column: x => x.ChildRequestId,
                        principalTable: "ChildRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AirDate = table.Column<DateTime>(nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    EpisodeNumber = table.Column<int>(nullable: false),
                    Requested = table.Column<bool>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpisodeRequests_SeasonRequests_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "SeasonRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonsContent_PlexContentId",
                table: "PlexSeasonsContent",
                column: "PlexContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildRequests_ParentRequestId",
                table: "ChildRequests",
                column: "ParentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildRequests_RequestedUserId",
                table: "ChildRequests",
                column: "RequestedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieIssues_IssueId",
                table: "MovieIssues",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieIssues_MovieId",
                table: "MovieIssues",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieRequests_RequestedUserId",
                table: "MovieRequests",
                column: "RequestedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TvIssues_IssueId",
                table: "TvIssues",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_TvIssues_TvId",
                table: "TvIssues",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserId",
                table: "Tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeRequests_SeasonId",
                table: "EpisodeRequests",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonRequests_ChildRequestId",
                table: "SeasonRequests",
                column: "ChildRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ApplicationConfiguration");

            migrationBuilder.DropTable(
                name: "Audit");

            migrationBuilder.DropTable(
                name: "GlobalSettings");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "PlexEpisode");

            migrationBuilder.DropTable(
                name: "PlexSeasonsContent");

            migrationBuilder.DropTable(
                name: "RadarrCache");

            migrationBuilder.DropTable(
                name: "MovieIssues");

            migrationBuilder.DropTable(
                name: "TvIssues");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "EpisodeRequests");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PlexContent");

            migrationBuilder.DropTable(
                name: "MovieRequests");

            migrationBuilder.DropTable(
                name: "SeasonRequests");

            migrationBuilder.DropTable(
                name: "ChildRequests");

            migrationBuilder.DropTable(
                name: "TvRequests");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
