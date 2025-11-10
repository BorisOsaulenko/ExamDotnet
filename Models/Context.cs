using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Models;

public class ApplicationDbContext : IdentityDbContext<User>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly ValueConverter<ICollection<string>, string> StringCollectionConverter =
        new(
            value => JsonSerializer.Serialize(value ?? Array.Empty<string>(), JsonOptions),
            value =>
                string.IsNullOrWhiteSpace(value)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(value, JsonOptions)
                        ?? new List<string>()
        );

    private static readonly ValueComparer<ICollection<string>> StringCollectionComparer = new(
        (left, right) =>
            (left ?? Array.Empty<string>()).SequenceEqual(right ?? Array.Empty<string>()),
        value =>
            value == null
                ? 0
                : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        value => value == null ? new List<string>() : new List<string>(value)
    );

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Image> Images => Set<Image>();
    public DbSet<ImageStats> ImageStats => Set<ImageStats>();
    public DbSet<ImageComment> ImageComments => Set<ImageComment>();
    public DbSet<ImageCollection> ImageCollections => Set<ImageCollection>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<UserConsumerHistory> UserConsumerHistories => Set<UserConsumerHistory>();
    public DbSet<UserProducerHistory> UserProducerHistories => Set<UserProducerHistory>();
    public DbSet<ImageAllowedUser> ImageAllowedUsers => Set<ImageAllowedUser>();
    public DbSet<ImageCollectionAllowedUser> ImageCollectionAllowedUsers =>
        Set<ImageCollectionAllowedUser>();
    public DbSet<UserFavoriteTag> UserFavoriteTags => Set<UserFavoriteTag>();
    public DbSet<ImageTag> ImageTags => Set<ImageTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureUser(builder);
        ConfigureImage(builder);
        ConfigureImageCollection(builder);
        ConfigureUserPreferences(builder);
        ConfigureUserActivity(builder);
    }

    private static void ConfigureUser(ModelBuilder builder)
    {
        builder
            .Entity<User>()
            .HasOne(u => u.Preferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureImage(ModelBuilder builder)
    {
        builder.Entity<Image>(entity =>
        {
            entity
                .HasOne(e => e.User)
                .WithMany(u => u.Images)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(e => e.ImageCollection)
                .WithMany(c => c.Images)
                .HasForeignKey(e => e.ImageCollectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(e => e.Stats)
                .WithOne(s => s.Image)
                .HasForeignKey<ImageStats>(s => s.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(e => e.AllowedUsers)
                .WithOne(au => au.Image)
                .HasForeignKey(au => au.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(e => e.Tags)
                .WithOne(t => t.Image)
                .HasForeignKey(t => t.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ImageAllowedUser>(entity =>
        {
            entity.HasIndex(e => new { e.ImageId, e.UserId }).IsUnique();
        });

        builder.Entity<ImageTag>(entity =>
        {
            entity.HasIndex(e => new { e.ImageId, e.Tag }).IsUnique();
        });
    }

    private static void ConfigureImageCollection(ModelBuilder builder)
    {
        builder.Entity<ImageCollection>(entity =>
        {
            entity
                .HasOne(e => e.User)
                .WithMany(u => u.ImageCollections)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(e => e.Images)
                .WithOne(i => i.ImageCollection)
                .HasForeignKey(i => i.ImageCollectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(e => e.CoverImage)
                .WithMany()
                .HasForeignKey(e => e.CoverImageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasMany(e => e.AllowedUsers)
                .WithOne(au => au.ImageCollection)
                .HasForeignKey(au => au.ImageCollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Subscribers).WithMany(p => p.SubscribedCollections);
        });

        builder.Entity<ImageCollectionAllowedUser>(entity =>
        {
            entity.HasIndex(e => new { e.ImageCollectionId, e.UserId }).IsUnique();
        });
    }

    private static void ConfigureUserPreferences(ModelBuilder builder)
    {
        builder.Entity<UserPreferences>(entity =>
        {
            entity
                .Property(e => e.FavoriteAuthors)
                .HasConversion(StringCollectionConverter)
                .Metadata.SetValueComparer(StringCollectionComparer);

            entity
                .HasMany(e => e.FavoriteTags)
                .WithOne(t => t.UserPreferences)
                .HasForeignKey(t => t.UserPreferencesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserFavoriteTag>(entity =>
        {
            entity.HasIndex(e => new { e.UserPreferencesId, e.Tag }).IsUnique();
        });
    }

    private static void ConfigureUserActivity(ModelBuilder builder)
    {
        builder.Entity<UserConsumerHistory>(entity =>
        {
            entity
                .HasOne(e => e.User)
                .WithMany(u => u.ConsumerHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(e => e.Image)
                .WithMany()
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(e => e.Collection)
                .WithMany()
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<UserProducerHistory>(entity =>
        {
            entity
                .HasOne(e => e.User)
                .WithMany(u => u.ProducerHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
