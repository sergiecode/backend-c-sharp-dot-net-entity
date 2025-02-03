using BackendUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; } // Agregar la tabla Roles

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed de roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("a0eeb8c1-bcdc-47c8-a48a-7e4d79e593a4"), Name = "Admin" },
            new Role { Id = Guid.Parse("b6b6c56e-416a-4b53-8fc6-b1eac0887c0e"), Name = "User" }
        );


    }


}