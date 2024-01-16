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
            .HasKey(i => new { i.BoardGameId, i.DomainId });

            modelBuilder.Entity<BoardGames_Mechanics>()
            .HasKey(i => new { i.BoardGameId, i.MechanicId });
        }
        /*Composite primary keys are supported in EF Core, but they do require an
        additional setting that is currently only supported by Fluent API. Let's
        quickly add that to our code before proceeding*/

    }
}
