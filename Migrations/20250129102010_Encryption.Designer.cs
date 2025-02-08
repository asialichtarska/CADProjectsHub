﻿// <auto-generated />
using System;
using CADProjectsHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CADProjectsHub.Migrations
{
    [DbContext(typeof(CADProjectsContext))]
    [Migration("20250129102010_Encryption")]
    partial class Encryption
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CADProjectsHub.Models.Assignment", b =>
                {
                    b.Property<int>("AssignmentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AssignmentID"));

                    b.Property<int>("CADModelID")
                        .HasColumnType("int");

                    b.Property<int>("ProjectID")
                        .HasColumnType("int");

                    b.HasKey("AssignmentID");

                    b.HasIndex("CADModelID");

                    b.HasIndex("ProjectID");

                    b.ToTable("Assignment", (string)null);
                });

            modelBuilder.Entity("CADProjectsHub.Models.CADModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<DateTime>("AssignmentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ConstructorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IVKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Manufacturing")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("CADModel", (string)null);
                });

            modelBuilder.Entity("CADProjectsHub.Models.Project", b =>
                {
                    b.Property<int>("ProjectID")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectManager")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProjectID");

                    b.ToTable("Project", (string)null);
                });

            modelBuilder.Entity("CADProjectsHub.Models.Assignment", b =>
                {
                    b.HasOne("CADProjectsHub.Models.CADModel", "CADModel")
                        .WithMany("Assignments")
                        .HasForeignKey("CADModelID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CADProjectsHub.Models.Project", "Project")
                        .WithMany("Assignments")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CADModel");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("CADProjectsHub.Models.CADModel", b =>
                {
                    b.Navigation("Assignments");
                });

            modelBuilder.Entity("CADProjectsHub.Models.Project", b =>
                {
                    b.Navigation("Assignments");
                });
#pragma warning restore 612, 618
        }
    }
}
