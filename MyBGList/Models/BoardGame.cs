using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBGList.Models
{
    [Table("BoardGames")] //this attribute instructs EF Core to use the specified name for the DB table related to this entity.
    public class BoardGame
    {
        [Key]/*set this field as the table's Primary Key*/
        [Required]
        public int Id { get; set; }

        //[Required]
        //public int PublisherId { get; set; } // adding the PublisherId property (int, required) to the BoardGame entity;


        [Required] //they won't accept a null-value.
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        [Required]
        public int Year { get; set; }
        [Required]
        public int MinPlayers { get; set; }
        [Required]
        public int MaxPlayers { get; set; }
        [Required]
        public int PlayTime { get; set; }
        [Required]
        public int MinAge { get; set; }
        [Required]
        public int UsersRated { get; set; }

        [Required]
        [Precision(4, 2)]
        public decimal RatingAverage { get; set; }
        [Required]
        public int BGGRank { get; set; }
        [Required]
        [Precision(4, 2)]
        public decimal ComplexityAverage { get; set; }
        [Required]
        public int OwnedUsers { get; set; }

        //[MaxLength(200)]
        //public string AlternateNames { get; set; } = null!;

        //[MaxLength(200)]
        //public string Designer { get; set; } = null!;

        //[Required]
        //public int Flags { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }

        //public Publisher? Publisher { get; set; }

        //additional properties added:
        public ICollection<BoardGames_Domains>? BoardGames_Domains { get; set; }
        public ICollection<BoardGames_Mechanics>? BoardGames_Mechanics { get; set; }
        //public ICollection<BoardGames_Categories>? BoardGames_Categories { get; set; }

    }
}