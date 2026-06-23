using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Story> Stories { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<PodCast> PodCasts { get; set; }
        
        // Cấu trúc tạp chí
        public DbSet<MagazineIssue> MagazineIssues { get; set; }
        public DbSet<MagazineArticle> MagazineArticles { get; set; }
    }
}
