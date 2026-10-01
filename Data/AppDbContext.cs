using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data;

/*
 * AppDbContext.cs
 * 
 * This class represents the application's database context. It inherits from DbContext and provides DbSet properties for the AppUser and AppRole entities. 
 * It also configures the model using the OnModelCreating method, applying configurations from the assembly.
 */
public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<AppRole> AppRoles => Set<AppRole>();

    /*
     * OnModelCreating
     * 
     * This method is called when the model for a derived context has been initialized, but before the model has been locked down and used to initialize the context. 
     * It allows further configuration of the model that was discovered by convention from the entity types exposed in DbSet properties on your derived context.
     */
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly
        );
    }
}