using System.ComponentModel.DataAnnotations;

namespace MyBGList.Models
{
    public class BoardGames_Domains /*As we can see, this time we didn't use the [Table("<name>")] attribute, since
the Conventions naming standards are good enough for this table.*/
    {
        [Key]
        [Required]
        public int BoardGameId { get; set; }

        [Key]
        [Required]
        public int DomainId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /*add the required navigation properties to Entities.*/
        public BoardGame? BoardGame { get; set; }

        public Domain? Domain { get; set; }
    }
}