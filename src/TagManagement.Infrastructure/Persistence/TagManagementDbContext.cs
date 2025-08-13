using Microsoft.EntityFrameworkCore;
using TagManagement.Infrastructure.Persistence.Models;

namespace TagManagement.Infrastructure.Persistence
{
    public class TagManagementDbContext : DbContext
    {
        public TagManagementDbContext(DbContextOptions<TagManagementDbContext> options) : base(options)
        {
        }

        // Main TDOC tables we need for tag management
        public DbSet<TagsModel> Tags { get; set; }
        public DbSet<TagContentModel> TagContents { get; set; }
        public DbSet<TagTypeModel> TagTypes { get; set; }
        public DbSet<UnitModel> Units { get; set; }
        public DbSet<ItemModel> Items { get; set; }
        public DbSet<CustomerModel> Customers { get; set; }
        public DbSet<LocationModel> Locations { get; set; }
        public DbSet<IndicatorModel> Indicators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Use the existing TDOC EF Core configurations
            // These models are already configured in the Foundation.Core library
            // We'll add any additional configuration needed for our tag management scenarios
            
            // Configure entity relationships and indexes
            ConfigureTagsModel(modelBuilder);
            ConfigureTagContentModel(modelBuilder);
            ConfigureRelationships(modelBuilder);

            modelBuilder.Entity<TagContentModel>(entity =>
            {
                // Additional indexes for tag content queries
                entity.HasIndex(e => e.ParentTagKeyId).HasDatabaseName("IX_TagContent_ParentTag");
                entity.HasIndex(e => e.LocationKeyId).HasDatabaseName("IX_TagContent_Location");
            });

            modelBuilder.Entity<UnitModel>(entity =>
            {
                // Additional indexes for unit queries
                entity.HasIndex(e => e.LocationKeyId).HasDatabaseName("IX_Units_Location");
            });

            modelBuilder.Entity<ItemModel>(entity =>
            {
                // Additional indexes for item queries
                entity.HasIndex(e => e.CustomerKeyId).HasDatabaseName("IX_Items_Customer");
            });
        }

        private void ConfigureTagsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TagsModel>(entity =>
            {
                // Additional indexes for tag management queries
                entity.HasIndex(e => new { e.TagNumber, e.TagTypeKeyId }).HasDatabaseName("IX_Tags_TagNumber_TagType");
                entity.HasIndex(e => e.LocationKeyId).HasDatabaseName("IX_Tags_Location");
                entity.HasIndex(e => e.ProcessBatchKeyId).HasDatabaseName("IX_Tags_ProcessBatch");
                entity.HasIndex(e => e.IsAutoTag).HasDatabaseName("IX_Tags_IsAutoTag");
            });
        }

        private void ConfigureTagContentModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TagContentModel>(entity =>
            {
                // Foreign key relationships
                entity.HasOne(tc => tc.ParentTag)
                    .WithMany(t => t.TagContents)
                    .HasForeignKey(tc => tc.ParentTagKeyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tc => tc.ChildTag)
                    .WithMany()
                    .HasForeignKey(tc => tc.ChildTagKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tc => tc.Unit)
                    .WithMany(u => u.TagContents)
                    .HasForeignKey(tc => tc.UnitKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tc => tc.Item)
                    .WithMany(i => i.TagContents)
                    .HasForeignKey(tc => tc.ItemKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tc => tc.Location)
                    .WithMany(l => l.TagContents)
                    .HasForeignKey(tc => tc.LocationKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tc => tc.Indicator)
                    .WithMany(ind => ind.TagContents)
                    .HasForeignKey(tc => tc.IndicatorKeyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // TagsModel relationships
            modelBuilder.Entity<TagsModel>(entity =>
            {
                entity.HasOne(t => t.Location)
                    .WithMany(l => l.Tags)
                    .HasForeignKey(t => t.LocationKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.TagType)
                    .WithMany(tt => tt.Tags)
                    .HasForeignKey(t => t.TagTypeKeyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // LocationModel self-referencing relationship
            modelBuilder.Entity<LocationModel>(entity =>
            {
                entity.HasOne(l => l.ParentLocation)
                    .WithMany(l => l.ChildLocations)
                    .HasForeignKey(l => l.ParentLocationKeyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // UnitModel relationships
            modelBuilder.Entity<UnitModel>(entity =>
            {
                entity.HasOne(u => u.Location)
                    .WithMany(l => l.Units)
                    .HasForeignKey(u => u.LocationKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(u => u.Item)
                    .WithMany(i => i.Units)
                    .HasForeignKey(u => u.ItemKeyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(u => u.Customer)
                    .WithMany(c => c.Units)
                    .HasForeignKey(u => u.CustomerKeyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ItemModel relationships
            modelBuilder.Entity<ItemModel>(entity =>
            {
                entity.HasOne(i => i.Customer)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => i.CustomerKeyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
