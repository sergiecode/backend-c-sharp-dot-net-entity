using System.ComponentModel.DataAnnotations;  // Importa las clases necesarias para realizar validaciones de datos, como Required, MaxLength, etc.

namespace BackendUsuarios.Models;  // Define el espacio de nombres para los modelos de datos.
public class Role
{
    public int Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(20)]
    public required string Name { get; set; }
}
