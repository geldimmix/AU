using AlgoritmaUzmani.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<SeoTag> SeoTags => Set<SeoTag>();
    public DbSet<GuideTag> GuideTags => Set<GuideTag>();
    public DbSet<GuideSeoTag> GuideSeoTags => Set<GuideSeoTag>();
    public DbSet<RelatedGuide> RelatedGuides => Set<RelatedGuide>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<StaticPage> StaticPages => Set<StaticPage>();
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.SlugTr).IsUnique();
            entity.HasIndex(e => e.SlugEn).IsUnique().HasFilter("\"SlugEn\" IS NOT NULL");
        });

        // Guide
        modelBuilder.Entity<Guide>(entity =>
        {
            entity.HasIndex(e => e.SlugTr).IsUnique();
            entity.HasIndex(e => e.SlugEn).IsUnique().HasFilter("\"SlugEn\" IS NOT NULL");
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFeatured);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Guides)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GuideTag (many-to-many)
        modelBuilder.Entity<GuideTag>(entity =>
        {
            entity.HasKey(e => new { e.GuideId, e.TagId });

            entity.HasOne(e => e.Guide)
                .WithMany(g => g.GuideTags)
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.GuideTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GuideSeoTag (many-to-many)
        modelBuilder.Entity<GuideSeoTag>(entity =>
        {
            entity.HasKey(e => new { e.GuideId, e.SeoTagId });

            entity.HasOne(e => e.Guide)
                .WithMany(g => g.GuideSeoTags)
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SeoTag)
                .WithMany(t => t.GuideSeoTags)
                .HasForeignKey(e => e.SeoTagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RelatedGuide (self-referencing many-to-many)
        modelBuilder.Entity<RelatedGuide>(entity =>
        {
            entity.HasKey(e => new { e.GuideId, e.RelatedGuideId });

            entity.HasOne(e => e.Guide)
                .WithMany(g => g.RelatedGuides)
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Related)
                .WithMany(g => g.RelatedToGuides)
                .HasForeignKey(e => e.RelatedGuideId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.SlugTr).IsUnique();
        });

        // AdminUser
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // StaticPage
        modelBuilder.Entity<StaticPage>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // VisitorLog
        modelBuilder.Entity<VisitorLog>(entity =>
        {
            entity.HasIndex(e => e.VisitedAt);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.SessionId);
        });

        // SiteSetting
        modelBuilder.Entity<SiteSetting>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}

