using Microsoft.EntityFrameworkCore;
using BackendUsuarios.Models.Products;
using BackendUsuarios.Models.Categories;
using BackendUsuarios.Models.Stocks;
using BackendUsuarios.Models.Suppliers;
using BackendUsuarios.Models.ProductTypes;
using BackendUsuarios.Models.Roles;
using BackendUsuarios.Models.Users;

namespace BackendUsuarios.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Insertar datos en la tabla Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = new Guid("d1e0bba2-84f1-4b59-a8c9-52792f8d8e2f"), Name = "Electronics" },
                new Category { Id = new Guid("b44e2eeb-f674-4043-8cf3-3f1d0c6f94a1"), Name = "Clothing" }
            );

            // Insertar datos en la tabla ProductTypes
            modelBuilder.Entity<ProductType>().HasData(
                new ProductType { Id = new Guid("c7587c0e-017b-4283-a158-4c24b6a8c2b7"), Description = "Smartphones", Name = "Mobile", isActive = true },
                new ProductType { Id = new Guid("fa30cf38-7b80-4fc8-8a6a-0c98e343c842"), Description = "Apparel", Name = "Fashion", isActive = true }
            );


            // Insertar datos en la tabla Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = new Guid("5fe52c48-6ac9-4b5d-b8cd-cc82396e10a9"), Email = "supplier1@example.com", Name = "Tech Supplier", Phone = "123-456-7890" },
                new Supplier { Id = new Guid("38964a9a-bde0-4c6d-8880-bd3d53b0e89f"), Email = "supplier2@example.com", Name = "Clothing Supplier", Phone = "987-654-3210" }
            );
            // Definir una fecha fija en UTC
            DateTime fixedUtcDate = new DateTime(2023, 12, 31, 12, 0, 0, DateTimeKind.Utc);

            // Insertar datos con valores fijos
            modelBuilder.Entity<Stock>().HasData(
                new Stock
                {
                    Id = new Guid("a84a5981-1747-429f-a413-05613d957fcd"),
                    Quantity = 150,
                    UpdateDate = fixedUtcDate // Valor de fecha estático
                },
                new Stock
                {
                    Id = new Guid("d786ef4f-2a4c-4f45-85db-bf7a5b44d9fe"),
                    Quantity = 200,
                    UpdateDate = fixedUtcDate // Valor de fecha estático
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = new Guid("a59e51f7-3b1d-44fd-8682-b9413c8a728f"),
                    CategoryId = new Guid("d1e0bba2-84f1-4b59-a8c9-52792f8d8e2f"),
                    CreationDate = fixedUtcDate, // Valor de fecha estático
                    Description = "Último modelo de Smartphone",
                    Name = "iPhone 13",
                    Price = 999.99m,
                    ProductTypeId = new Guid("c7587c0e-017b-4283-a158-4c24b6a8c2b7"),
                    StockId = new Guid("a84a5981-1747-429f-a413-05613d957fcd"),
                    SupplierId = new Guid("5fe52c48-6ac9-4b5d-b8cd-cc82396e10a9")
                },
                new Product
                {
                    Id = new Guid("b12de1fb-1b4c-4e92-8221-8e380bb059f5"),
                    CategoryId = new Guid("b44e2eeb-f674-4043-8cf3-3f1d0c6f94a1"),
                    CreationDate = fixedUtcDate, // Valor de fecha estático
                    Description = "Camiseta estilosa",
                    Name = "Camiseta XL",
                    Price = 19.99m,
                    ProductTypeId = new Guid("fa30cf38-7b80-4fc8-8a6a-0c98e343c842"),
                    StockId = new Guid("d786ef4f-2a4c-4f45-85db-bf7a5b44d9fe"),
                    SupplierId = new Guid("38964a9a-bde0-4c6d-8880-bd3d53b0e89f")
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
