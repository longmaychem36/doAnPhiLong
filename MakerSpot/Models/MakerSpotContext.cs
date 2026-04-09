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

        // Phase 4
        public DbSet<Follower> Followers { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<CollectionItem> CollectionItems { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Phase 10: Reports & AuditLogs
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;

        // Phase 13: Forum
        public DbSet<ForumPost> ForumPosts { get; set; } = null!;
        public DbSet<ForumReply> ForumReplies { get; set; } = null!;
        
        // Admin: Moderator Topics
        public DbSet<ModeratorTopic> ModeratorTopics { get; set; } = null!;

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

            // Phase 4: Followers
            modelBuilder.Entity<Follower>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.FollowerUser)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.FollowingUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Phase 4: Collections
            modelBuilder.Entity<Collection>()
                .HasOne(c => c.User)
                .WithMany(u => u.Collections)
                .HasForeignKey(c => c.UserId);

            // Phase 4: CollectionItems
            modelBuilder.Entity<CollectionItem>()
                .HasKey(ci => new { ci.CollectionId, ci.ProductId });

            modelBuilder.Entity<CollectionItem>()
                .HasOne(ci => ci.Collection)
                .WithMany(c => c.CollectionItems)
                .HasForeignKey(ci => ci.CollectionId);

            modelBuilder.Entity<CollectionItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Phase 4: Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);

            // Phase 10: Reports
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReporterUser)
                .WithMany()
                .HasForeignKey(r => r.ReporterUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Phase 10: AuditLogs
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Phase 11: Chat Feature
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.SharedProduct)
                .WithMany()
                .HasForeignKey(m => m.SharedProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // Phase 13: Forum
            modelBuilder.Entity<ForumPost>()
                .HasOne(fp => fp.User)
                .WithMany()
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ForumReply>()
                .HasOne(fr => fr.ForumPost)
                .WithMany(fp => fp.Replies)
                .HasForeignKey(fr => fr.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumReply>()
                .HasOne(fr => fr.User)
                .WithMany()
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Moderator Topics
            modelBuilder.Entity<ModeratorTopic>()
                .HasKey(mt => new { mt.UserId, mt.TopicId });

            modelBuilder.Entity<ModeratorTopic>()
                .HasOne(mt => mt.User)
                .WithMany()
                .HasForeignKey(mt => mt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ModeratorTopic>()
                .HasOne(mt => mt.Topic)
                .WithMany()
                .HasForeignKey(mt => mt.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
