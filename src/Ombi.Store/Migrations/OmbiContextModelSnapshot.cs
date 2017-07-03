using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Ombi.Store.Context;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Migrations
{
    [DbContext(typeof(OmbiContext))]
    partial class OmbiContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("Ombi.Store.Entities.GlobalSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<string>("SettingsName");

                    b.HasKey("Id");

                    b.ToTable("GlobalSettings");
                });

            modelBuilder.Entity("Ombi.Store.Entities.NotificationTemplates", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Agent");

                    b.Property<bool>("Enabled");

                    b.Property<string>("Message");

                    b.Property<int>("NotificationType");

                    b.Property<string>("Subject");

                    b.HasKey("Id");

                    b.ToTable("NotificationTemplates");
                });

            modelBuilder.Entity("Ombi.Store.Entities.PlexContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AddedAt");

                    b.Property<string>("Key");

                    b.Property<string>("ProviderId");

                    b.Property<string>("ReleaseYear");

                    b.Property<string>("Title");

                    b.Property<int>("Type");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("PlexContent");
                });

            modelBuilder.Entity("Ombi.Store.Entities.PlexSeasonsContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ParentKey");

                    b.Property<int>("PlexContentId");

                    b.Property<int>("SeasonKey");

                    b.Property<int>("SeasonNumber");

                    b.HasKey("Id");

                    b.HasIndex("PlexContentId");

                    b.ToTable("PlexSeasonsContent");
                });

            modelBuilder.Entity("Ombi.Store.Entities.RadarrCache", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TheMovieDbId");

                    b.HasKey("Id");

                    b.ToTable("RadarrCache");
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.ChildRequests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Approved");

                    b.Property<bool>("Available");

                    b.Property<bool?>("Denied");

                    b.Property<string>("DeniedReason");

                    b.Property<int?>("IssueId");

                    b.Property<int>("ParentRequestId");

                    b.Property<int>("RequestType");

                    b.Property<DateTime>("RequestedDate");

                    b.Property<int>("RequestedUserId");

                    b.HasKey("Id");

                    b.HasIndex("ParentRequestId");

                    b.HasIndex("RequestedUserId");

                    b.ToTable("ChildRequests");
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.MovieIssues", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<int?>("IssueId");

                    b.Property<int>("MovieId");

                    b.Property<string>("Subect");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.HasIndex("MovieId");

                    b.ToTable("MovieIssues");
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.MovieRequests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Approved");

                    b.Property<bool>("Available");

                    b.Property<bool?>("Denied");

                    b.Property<string>("DeniedReason");

                    b.Property<string>("ImdbId");

                    b.Property<int?>("IssueId");

                    b.Property<string>("Overview");

                    b.Property<string>("PosterPath");

                    b.Property<DateTime>("ReleaseDate");

                    b.Property<int>("RequestType");

                    b.Property<DateTime>("RequestedDate");

                    b.Property<int>("RequestedUserId");

                    b.Property<string>("Status");

                    b.Property<int>("TheMovieDbId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("RequestedUserId");

                    b.ToTable("MovieRequests");
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.TvIssues", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<int?>("IssueId");

                    b.Property<string>("Subect");

                    b.Property<int>("TvId");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.HasIndex("TvId");

                    b.ToTable("TvIssues");
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.TvRequests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ImdbId");

                    b.Property<string>("Overview");

                    b.Property<string>("PosterPath");

                    b.Property<DateTime>("ReleaseDate");

                    b.Property<int?>("RootFolder");

                    b.Property<string>("Status");

                    b.Property<string>("Title");

                    b.Property<int>("TvDbId");

                    b.HasKey("Id");

                    b.ToTable("TvRequests");
                });

            modelBuilder.Entity("Ombi.Store.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Alias");

                    b.Property<string>("ClaimsSerialized");

                    b.Property<string>("EmailAddress");

                    b.Property<string>("Password");

                    b.Property<byte[]>("Salt");

                    b.Property<int>("UserType");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Ombi.Store.Repository.Requests.EpisodeRequests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AirDate");

                    b.Property<bool>("Approved");

                    b.Property<bool>("Available");

                    b.Property<int>("EpisodeNumber");

                    b.Property<bool>("Requested");

                    b.Property<int>("SeasonId");

                    b.Property<string>("Title");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("SeasonId");

                    b.ToTable("EpisodeRequests");
                });

            modelBuilder.Entity("Ombi.Store.Repository.Requests.SeasonRequests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChildRequestId");

                    b.Property<int>("SeasonNumber");

                    b.HasKey("Id");

                    b.HasIndex("ChildRequestId");

                    b.ToTable("SeasonRequests");
                });

            modelBuilder.Entity("Ombi.Store.Entities.PlexSeasonsContent", b =>
                {
                    b.HasOne("Ombi.Store.Entities.PlexContent")
                        .WithMany("Seasons")
                        .HasForeignKey("PlexContentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.ChildRequests", b =>
                {
                    b.HasOne("Ombi.Store.Entities.Requests.TvRequests", "ParentRequest")
                        .WithMany("ChildRequests")
                        .HasForeignKey("ParentRequestId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Ombi.Store.Entities.User", "RequestedUser")
                        .WithMany()
                        .HasForeignKey("RequestedUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.MovieIssues", b =>
                {
                    b.HasOne("Ombi.Store.Entities.Requests.MovieRequests")
                        .WithMany("Issues")
                        .HasForeignKey("IssueId");

                    b.HasOne("Ombi.Store.Entities.Requests.MovieRequests", "Movie")
                        .WithMany()
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.MovieRequests", b =>
                {
                    b.HasOne("Ombi.Store.Entities.User", "RequestedUser")
                        .WithMany()
                        .HasForeignKey("RequestedUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Entities.Requests.TvIssues", b =>
                {
                    b.HasOne("Ombi.Store.Entities.Requests.ChildRequests")
                        .WithMany("Issues")
                        .HasForeignKey("IssueId");

                    b.HasOne("Ombi.Store.Entities.Requests.ChildRequests", "Child")
                        .WithMany()
                        .HasForeignKey("TvId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Repository.Requests.EpisodeRequests", b =>
                {
                    b.HasOne("Ombi.Store.Repository.Requests.SeasonRequests", "Season")
                        .WithMany("Episodes")
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ombi.Store.Repository.Requests.SeasonRequests", b =>
                {
                    b.HasOne("Ombi.Store.Entities.Requests.ChildRequests", "ChildRequest")
                        .WithMany("SeasonRequests")
                        .HasForeignKey("ChildRequestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
