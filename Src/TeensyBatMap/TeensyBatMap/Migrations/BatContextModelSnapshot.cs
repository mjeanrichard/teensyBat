using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using TeensyBatMap.Database;

namespace TeensyBatMap.Migrations
{
    [DbContext(typeof(BatContext))]
    partial class BatContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("TeensyBatMap.Domain.BatCall", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("AvgFrequency");

                    b.Property<int>("BatNodeLogId");

                    b.Property<int>("ClippedSamples");

                    b.Property<uint>("DcOffset");

                    b.Property<uint>("Duration");

                    b.Property<bool>("Enabled");

                    b.Property<byte[]>("FftData");

                    b.Property<uint>("MaxFrequency");

                    b.Property<uint>("MaxPower");

                    b.Property<int>("MissedSamples");

                    b.Property<byte[]>("PowerData");

                    b.Property<DateTime>("StartTime");

                    b.Property<uint>("StartTimeMs");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "BatCalls");
                });

            modelBuilder.Entity("TeensyBatMap.Domain.BatInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BatNodeLogId");

                    b.Property<uint>("BatteryVoltage");

                    b.Property<uint>("SampleDuration");

                    b.Property<DateTime>("Time");

                    b.Property<uint>("TimeMs");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "BatInfos");
                });

            modelBuilder.Entity("TeensyBatMap.Domain.BatNodeLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CallCount");

                    b.Property<string>("Description");

                    b.Property<int?>("FirstCallId");

                    b.Property<int?>("LastCallId");

                    b.Property<double>("Latitude");

                    b.Property<DateTime>("LogStart");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name");

                    b.Property<int>("NodeId");

                    b.Property<int>("Verison");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "BatNodeLogs");
                });

            modelBuilder.Entity("TeensyBatMap.Domain.BatCall", b =>
                {
                    b.HasOne("TeensyBatMap.Domain.BatNodeLog")
                        .WithMany()
                        .HasForeignKey("BatNodeLogId");
                });

            modelBuilder.Entity("TeensyBatMap.Domain.BatInfo", b =>
                {
                    b.HasOne("TeensyBatMap.Domain.BatNodeLog")
                        .WithMany()
                        .HasForeignKey("BatNodeLogId");
                });

            modelBuilder.Entity("TeensyBatMap.Domain.BatNodeLog", b =>
                {
                    b.HasOne("TeensyBatMap.Domain.BatCall")
                        .WithMany()
                        .HasForeignKey("FirstCallId");

                    b.HasOne("TeensyBatMap.Domain.BatCall")
                        .WithMany()
                        .HasForeignKey("LastCallId");
                });
        }
    }
}
