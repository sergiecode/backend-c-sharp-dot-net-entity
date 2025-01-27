using BackendUsuarios.Models;  // Importa el espacio de nombres que contiene los modelos, en este caso, el modelo de User.
using Microsoft.EntityFrameworkCore;  // Importa las clases necesarias para trabajar con Entity Framework Core, que es el ORM para interactuar con la base de datos.

namespace BackendUsuarios.Data;  // Define el espacio de nombres para este archivo, que contiene la configuración del contexto de base de datos.

public class AppDbContext : DbContext  // Declara la clase AppDbContext que hereda de DbContext, lo que permite interactuar con la base de datos.
{
    // Constructor que recibe las opciones de configuración de la base de datos como parámetro e inicializa la clase base (DbContext).
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet que representa la tabla de usuarios en la base de datos. Este DbSet permitirá hacer operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre los usuarios.
    public DbSet<User> Users { get; set; }
}
