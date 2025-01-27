using System.ComponentModel.DataAnnotations;  // Importa las clases necesarias para realizar validaciones de datos, como Required, MaxLength, etc.

namespace BackendUsuarios.Models;  // Define el espacio de nombres para los modelos de datos.

public class User  // Declara la clase User que representa el modelo de usuario en la aplicación.
{
    public int Id { get; set; }  // Propiedad Id que representa el identificador único del usuario en la base de datos.

    // La propiedad Name, que es un campo de texto con validaciones de longitud y obligatoriedad.
    [Required(ErrorMessage = "The name is required.")]  // La validación asegura que el nombre sea obligatorio.
    [MaxLength(50, ErrorMessage = "The name can't exceed 50 characters.")]  // Limita la longitud del nombre a 50 caracteres.
    public string Name { get; set; }  // Propiedad Name que almacena el nombre del usuario.

    // La propiedad Email, que representa la dirección de correo electrónico del usuario con validaciones.
    [Required(ErrorMessage = "The email is required.")]  // La validación asegura que el correo electrónico sea obligatorio.
    [EmailAddress(ErrorMessage = "The email format is invalid.")]  // Valida que el correo tenga un formato correcto (ej. usuario@dominio.com).
    public string Email { get; set; }  // Propiedad Email que almacena el correo electrónico del usuario.
    
    // La propiedad Password, que representa la contraseña del usuario con validaciones.
    [Required(ErrorMessage = "The email is required.")]  // La validación asegura que la contraseña sea obligatoria.
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = "The password must have at least one lowercase letter, one uppercase letter, and one number.")]  // La contraseña debe tener al menos una letra minúscula, una mayúscula, un número y entre 6 y 50 caracteres.
    public string Password { get; set; }  // Propiedad Password que almacena la contraseña del usuario.
}
