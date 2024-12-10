using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Web.Models.Domain;
using System;
using System.Collections.Generic;

public class NewsAggregateDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public NewsAggregateDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<SavedArticle> SavedArticles { get; set; }
    public DbSet<Article> Articles { get; set; } // Add Article DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        base.OnModelCreating(modelBuilder); // calling base method for Identity configuration

        // Configure SearchHistory
        modelBuilder.Entity<SearchHistory>()
            .HasKey(sh => sh.SearchHistoryId);
        modelBuilder.Entity<SearchHistory>()
            .HasOne(sh => sh.User)
            .WithMany(u => u.SearchHistories)
            .HasForeignKey(sh => sh.UserId);

        // Configure SavedArticle
        modelBuilder.Entity<SavedArticle>()
            .HasKey(sa => sa.SavedArticleId);
        modelBuilder.Entity<SavedArticle>()
            .HasOne(sa => sa.User)
            .WithMany(u => u.SavedArticles)
            .HasForeignKey(sa => sa.UserId);

        // Configure Article
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(a => a.Id); // Primary key for Article
            entity.Property(a => a.Title)
                  .IsRequired()
                  .HasMaxLength(200); // Title is required, max length of 200
            entity.Property(a => a.Description)
                  .IsRequired()
                  .HasMaxLength(500); // Description is required, max length of 200
            entity.Property(a => a.Content)
                  .IsRequired(); // Content is required
            entity.Property(a => a.PublishedAt)
                  .IsRequired(); // PublishedDate is required
            entity.Property(a => a.Source)
                  .HasMaxLength(100); // Optional Author field, max length 100

            // Relationship with ApplicationUser (optional)
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Articles)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull); // If user is deleted, set UserId to null

            // Indexes for better performance
            entity.HasIndex(a => a.PublishedAt); // Index on PublishedDate
            entity.HasIndex(a => a.Title); // Index on Title for search optimization
        });

        // Seed roles (User, Admin, SuperAdmin)
        var adminRoleId = Guid.NewGuid();
        var superAdminRoleId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();
        var roles = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = adminRoleId.ToString()
            },
            new IdentityRole<Guid>
            {
                Id = superAdminRoleId,
                Name = "SuperAdmin",
                NormalizedName = "SUPERADMIN",
                ConcurrencyStamp = superAdminRoleId.ToString()
            },
            new IdentityRole<Guid>
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = userRoleId.ToString()
            }
        };
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(roles);

        // Seed SuperAdmin user
        var superAdminId = Guid.NewGuid();
        var superAdminUser = new ApplicationUser
        {
            Id = superAdminId,
            UserName = "NewsAggregatorSuperAdmin",
            NormalizedUserName = "NEWSAGGREGATORSUPERADMIN",
            Email = "superadmin@newsaggregator.com",
            NormalizedEmail = "SUPERADMIN@NEWSAGGREGATOR.COM",
            EmailConfirmed = true
        };

        // Creating password for the SuperAdmin
        superAdminUser.PasswordHash = hasher.HashPassword(superAdminUser, "superAdminId@564");
        modelBuilder.Entity<ApplicationUser>().HasData(superAdminUser);

        // Add all roles to SuperAdmin user
        var superAdminRoles = new List<IdentityUserRole<Guid>>
        {
            new IdentityUserRole<Guid>
            {
                RoleId = adminRoleId,
                UserId = superAdminId
            },
            new IdentityUserRole<Guid>
            {
                RoleId = superAdminRoleId,
                UserId = superAdminId
            },
            new IdentityUserRole<Guid>
            {
                RoleId = userRoleId,
                UserId = superAdminId
            }
        };
        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(superAdminRoles);
    }
}
