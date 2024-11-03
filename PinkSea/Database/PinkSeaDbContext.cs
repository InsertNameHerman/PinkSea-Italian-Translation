using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PinkSea.Database.Models;

namespace PinkSea.Database;

/// <summary>
/// The DB context.
/// </summary>
public class PinkSeaDbContext : DbContext
{
    /// <summary>
    /// The oekaki table.
    /// </summary>
    public DbSet<OekakiModel> Oekaki { get; set; } = null!;
    
    /// <summary>
    /// The tags table.
    /// </summary>
    public DbSet<TagModel> Tags { get; set; } = null!;
    
    /// <summary>
    /// The tag/oekaki relations.
    /// </summary>
    public DbSet<TagOekakiRelationModel> TagOekakiRelations { get; set; } = null!;
    
    /// <summary>
    /// The user table.
    /// </summary>
    public DbSet<UserModel> Users { get; set; } = null!;
    
    /// <summary>
    /// The Oekaki table.
    /// </summary>
    public DbSet<OAuthStateModel> OAuthStates { get; set; } = null!;

    /// <summary>
    /// The Oekaki table.
    /// </summary>
    public DbSet<ConfigurationModel> Configuration { get; set; } = null!;
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;
        
        // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
        // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
        // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
        // use the DateTimeOffsetToBinaryConverter
        // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
        // This only supports millisecond precision, but should be sufficient for most use cases.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                           || p.PropertyType == typeof(DateTimeOffset?));
            foreach (var property in properties)
            {
                modelBuilder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeOffsetToBinaryConverter());
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=pinksea.db");
}