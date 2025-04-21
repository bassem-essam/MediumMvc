using MediumMvc.Areas.Identity.Data;
using MediumMvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
        .Property(p => p.Id)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Follow>()
        .HasOne(f => f.Follower)
        .WithMany(u => u.Following)
        .HasForeignKey(f => f.FollowerId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        modelBuilder.Entity<Follow>()
        .HasOne(f => f.Followed)
        .WithMany(u => u.Followers)
        .HasForeignKey(f => f.FollowedId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete



        // modelBuilder.Entity<ApplicationUser>()
        // .HasOne(a => a.Author);

        // modelBuilder.Entity<Author>()
        //     .HasOne(a => a.User)
        //     .WithMany()
        //     .OnDelete(DeleteBehavior.Cascade); // Cascade delete if ApplicationUser is deleted

        // // Configure the many-to-one relationship between Post and Author
        // modelBuilder.Entity<Post>()
        //     .HasOne(p => p.Author)
        //     .WithMany(a => a.Posts)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}