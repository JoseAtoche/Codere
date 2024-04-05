using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Enumerados;
using Models.ShowModels.Repository;
using System.Globalization;

namespace Repository
{
    public class ShowsRepository : DbContext
    {
        string dbName;
        public DbSet<ShowDto> Shows { get; set; }
        public DbSet<NetworkDto> Networks { get; set; }
        public DbSet<CountryDto> Countries { get; set; }
        public DbSet<ScheduleDto> Schedules { get; set; }
        public DbSet<ExternalsDto> Externals { get; set; }
        public DbSet<ImageDto> Images { get; set; }
        public DbSet<LinkDto> Links { get; set; }

        public ShowsRepository()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            if (configuration.GetConnectionString("ConnectOption") == Enumerados.SQLConexion.SQLLite.ToString())
            {
                dbName = configuration.GetConnectionString("ShowsDatabase");
                Database.EnsureCreated();

            }
            else {
                dbName = configuration.GetConnectionString("ShowsDatabaseSQLServer");
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShowDto>()
            .HasOne(s => s.Schedule)
            .WithOne()
            .HasForeignKey<ShowDto>(s => s.IdSchedule);

            modelBuilder.Entity<ShowDto>()
                .HasOne(s => s.Network)
                .WithMany()
                .HasForeignKey(s => s.idNetwork);

            modelBuilder.Entity<ShowDto>()
                .HasOne(s => s.Externals)
                .WithOne()
                .HasForeignKey<ShowDto>(s => s.idExternals);

            modelBuilder.Entity<ShowDto>()
                .HasOne(s => s.Image)
                .WithOne()
                .HasForeignKey<ShowDto>(s => s.idImage);

            modelBuilder.Entity<ShowDto>()
                .HasOne(s => s.Link)
                .WithOne()
                .HasForeignKey<ShowDto>(s => s.idLink);

            modelBuilder.Entity<NetworkDto>()
                .HasOne(n => n.Country)
                .WithMany()
                .HasForeignKey(n => n.idCountry);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString;
            if (configuration.GetConnectionString("ConnectOption") == Enumerados.SQLConexion.SQLLite.ToString()) {

                connectionString = $"Data Source={dbName};";
                optionsBuilder.UseSqlite(connectionString)
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);

            } else {
                connectionString = $"{dbName};"; 
                optionsBuilder.UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);
            }            
        }

        public void AddShowInfos(List<ShowDto> showInfoList)
        {
            Shows.AddRange(showInfoList);
            SaveChanges();
        }

    }
}

