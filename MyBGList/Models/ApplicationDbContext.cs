using Microsoft.EntityFrameworkCore;

namespace MyBGList.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // TODO: custom code here
            /*that's the place where we can configure our model using the ModelBuilder API 
              (also known as Fluent API*/
            //instead of todo this code follows:

            modelBuilder.Entity<BoardGames_Domains>()
                .HasKey(i => new { i.BoardGameId, i.DomainId }); /* from : [Key] [Required]*/

            modelBuilder.Entity<BoardGames_Domains>()
                .HasOne(x => x.BoardGame)
                .WithMany(y => y.BoardGames_Domains)
                .HasForeignKey(f => f.BoardGameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BoardGames_Domains>()
                .HasOne(o => o.Domain)
                .WithMany(m => m.BoardGames_Domains)
                .HasForeignKey(f => f.DomainId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BoardGames_Mechanics>()
                .HasKey(i => new { i.BoardGameId, i.MechanicId });
            /*used the HasKey method to configure the composite primary key*/

            /*Composite primary keys are supported in EF Core, but they do require an
            additional setting that is currently only supported by Fluent API. Let's
            quickly add that to our code before proceeding*/

            modelBuilder.Entity<BoardGames_Mechanics>()
                .HasOne(x => x.BoardGame)
                .WithMany(y => y.BoardGames_Mechanics)
                .HasForeignKey(f => f.BoardGameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BoardGames_Mechanics>()
                .HasOne(o => o.Mechanic)
                .WithMany(m => m.BoardGames_Mechanics)
                .HasForeignKey(f => f.MechanicId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<BoardGame> BoardGames => Set<BoardGame>();
        public DbSet<Domain> Domains => Set<Domain>();
        public DbSet<Mechanic> Mechanics => Set<Mechanic>();
        public DbSet<BoardGames_Domains> BoardGames_Domains => Set<BoardGames_Domains>();
        public DbSet<BoardGames_Mechanics> BoardGames_Mechanics => Set <BoardGames_Mechanics>();
    }
}
