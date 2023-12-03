using Microsoft.EntityFrameworkCore;
using APICodere.Models.Dtos;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace APICodere.Repository
{
    public class ShowsRepository : DbContext
    {
        string dbName = "Tvshows.sqlite";
        public DbSet<ShowDto> Shows { get; set; }
        public DbSet<NetworkDto> Networks { get; set; }
        public DbSet<CountryDto> Countries { get; set; }
        public DbSet<ScheduleDto> Schedules { get; set; }
        public DbSet<ExternalsDto> Externals { get; set; }
        public DbSet<ImageDto> Images { get; set; }
        public DbSet<LinkDto> Links { get; set; }

        public ShowsRepository(DbContextOptions<ShowsRepository> options) : base(options)
        {
        } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //// Verifica la configuración de la entidad CountryDto
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

            // Mapeos para NetworkDto
            modelBuilder.Entity<NetworkDto>()
                .HasOne(n => n.Country)
                .WithMany()
                .HasForeignKey(n => n.idCountry);

        }      


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = $"Data Source={dbName};";
            optionsBuilder.UseSqlite(connectionString)
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
            ApplyMigrations();

        }
        private void ApplyMigrations()
        {
            try
            {
                // Obtén el migrator y aplica las migraciones
                var migrator = Database.GetService<IMigrator>();
                migrator.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al aplicar migraciones: " + ex.Message);
                // Puedes manejar el error de la manera que prefieras.
            }
        }

        public void AddShowInfos(List<ShowDto> showInfoList)
        {
            Shows.AddRange(showInfoList);
            SaveChanges();
        }

    }
}

