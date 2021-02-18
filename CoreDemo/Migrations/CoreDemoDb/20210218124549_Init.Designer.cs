﻿// <auto-generated />
using System;
using CoreDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreDemo.Migrations.CoreDemoDb
{
    [DbContext(typeof(CoreDemoDbContext))]
    [Migration("20210218124549_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("CoreDemo.Composers", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("YearOfBirth")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("YearOfDeath")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Composers");
                });

            modelBuilder.Entity("CoreDemo.Works", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ComposerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("YearOfComposition")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ComposerId");

                    b.ToTable("Works");
                });

            modelBuilder.Entity("CoreDemo.Works", b =>
                {
                    b.HasOne("CoreDemo.Composers", "Composer")
                        .WithMany("Works")
                        .HasForeignKey("ComposerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Composer");
                });

            modelBuilder.Entity("CoreDemo.Composers", b =>
                {
                    b.Navigation("Works");
                });
#pragma warning restore 612, 618
        }
    }
}
