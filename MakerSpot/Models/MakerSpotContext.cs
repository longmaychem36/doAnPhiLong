using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Models
{
    public class MakerSpotContext : DbContext
    {
        public MakerSpotContext(DbContextOptions<MakerSpotContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Topic> Topics { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductMedia> ProductMedia { get; set; } = null!;
        public DbSet<ProductTopic> ProductTopics { get; set; } = null!;
        public DbSet<ProductMaker> ProductMakers { get; set; } = null!;
        public DbSet<ProductUpvote> ProductUpvotes { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<CommentVote> CommentVotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserRoles: Composite Key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // ProductTopics: Composite Key
            modelBuilder.Entity<ProductTopic>()
                .HasKey(pt => new { pt.ProductId, pt.TopicId });

            modelBuilder.Entity<ProductTopic>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTopics)
                .HasForeignKey(pt => pt.ProductId);

            modelBuilder.Entity<ProductTopic>()
                .HasOne(pt => pt.Topic)
                .WithMany(t => t.ProductTopics)
                .HasForeignKey(pt => pt.TopicId);

            // ProductMakers: Composite Key
            modelBuilder.Entity<ProductMaker>()
                .HasKey(pm => new { pm.ProductId, pm.UserId });

            modelBuilder.Entity<ProductMaker>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMakers)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaker>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProductMakers)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProductUpvotes: Composite Key
            modelBuilder.Entity<ProductUpvote>()
                .HasKey(pu => new { pu.ProductId, pu.UserId });

            modelBuilder.Entity<ProductUpvote>()
                .HasOne(pu => pu.Product)
                .WithMany(p => p.ProductUpvotes)
                .HasForeignKey(pu => pu.ProductId);

            modelBuilder.Entity<ProductUpvote>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.ProductUpvotes)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // CommentVotes: Composite Key
            modelBuilder.Entity<CommentVote>()
                .HasKey(cv => new { cv.CommentId, cv.UserId });

            modelBuilder.Entity<CommentVote>()
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.CommentVotes)
                .HasForeignKey(cv => cv.CommentId);

            modelBuilder.Entity<CommentVote>()
                .HasOne(cv => cv.User)
                .WithMany(u => u.CommentVotes)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comments relationships
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            // User relations
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
