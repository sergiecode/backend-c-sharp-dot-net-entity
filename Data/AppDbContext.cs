using BackendUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}