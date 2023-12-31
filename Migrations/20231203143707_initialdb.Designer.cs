﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SatApiTest.Models;

#nullable disable

namespace SatApiTest.Migrations
{
    [DbContext(typeof(SatContext))]
    [Migration("20231203143707_initialdb")]
    partial class initialdb
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SatApiTest.Models.SelfAssessment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("Domain1Competency1")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency1Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain1Competency2")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency2Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain1Competency3")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency3Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain1Competency4")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency4Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain1Competency5")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency5Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain1Competency6")
                        .HasColumnType("int");

                    b.Property<bool>("Domain1Competency6Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain2Competency1")
                        .HasColumnType("int");

                    b.Property<bool>("Domain2Competency1Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain2Competency2")
                        .HasColumnType("int");

                    b.Property<bool>("Domain2Competency2Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain2Competency3")
                        .HasColumnType("int");

                    b.Property<bool>("Domain2Competency3Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain2Competency4")
                        .HasColumnType("int");

                    b.Property<bool>("Domain2Competency4Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain3Competency1")
                        .HasColumnType("int");

                    b.Property<bool>("Domain3Competency1Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain3Competency2")
                        .HasColumnType("int");

                    b.Property<bool>("Domain3Competency2Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain3Competency3")
                        .HasColumnType("int");

                    b.Property<bool>("Domain3Competency3Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain3Competency4")
                        .HasColumnType("int");

                    b.Property<bool>("Domain3Competency4Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain4Competency1")
                        .HasColumnType("int");

                    b.Property<bool>("Domain4Competency1Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain4Competency2")
                        .HasColumnType("int");

                    b.Property<bool>("Domain4Competency2Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain4Competency3")
                        .HasColumnType("int");

                    b.Property<bool>("Domain4Competency3Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain4Competency4")
                        .HasColumnType("int");

                    b.Property<bool>("Domain4Competency4Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain4Competency5")
                        .HasColumnType("int");

                    b.Property<bool>("Domain4Competency5Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain5Competency1")
                        .HasColumnType("int");

                    b.Property<bool>("Domain5Competency1Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain5Competency2")
                        .HasColumnType("int");

                    b.Property<bool>("Domain5Competency2Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain5Competency3")
                        .HasColumnType("int");

                    b.Property<bool>("Domain5Competency3Important")
                        .HasColumnType("bit");

                    b.Property<int>("Domain5Competency4")
                        .HasColumnType("int");

                    b.Property<bool>("Domain5Competency4Important")
                        .HasColumnType("bit");

                    b.Property<string>("Institution")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SelfAssessments");
                });
#pragma warning restore 612, 618
        }
    }
}
