using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class UserFavorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string ContentType { get; set; } // "Story" hoặc "Podcast"

        public int ContentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
