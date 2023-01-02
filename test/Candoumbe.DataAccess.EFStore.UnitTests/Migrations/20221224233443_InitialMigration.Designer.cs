﻿// <auto-generated />
using System;
using Candoumbe.DataAccess.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Candoumbe.DataAccess.EFStore.UnitTests.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20221224233443_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.12");

            modelBuilder.Entity("Candoumbe.DataAccess.Tests.Repositories.Acolyte", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("HeroId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HeroId");

                    b.ToTable("Acolytes");
                });

            modelBuilder.Entity("Candoumbe.DataAccess.Tests.Repositories.Hero", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Heroes");
                });

            modelBuilder.Entity("Candoumbe.DataAccess.Tests.Repositories.Acolyte", b =>
                {
                    b.HasOne("Candoumbe.DataAccess.Tests.Repositories.Hero", null)
                        .WithMany("Acolytes")
                        .HasForeignKey("HeroId");
                });

            modelBuilder.Entity("Candoumbe.DataAccess.Tests.Repositories.Hero", b =>
                {
                    b.Navigation("Acolytes");
                });
#pragma warning restore 612, 618
        }
    }
}
