using BackendUsuarios.Data;  // Importa el espacio de nombres que contiene el contexto de la base de datos (AppDbContext).
using BackendUsuarios.Models;  // Importa el espacio de nombres que contiene los modelos (por ejemplo, el modelo de User).
using Microsoft.AspNetCore.Mvc;  // Importa las clases necesarias para trabajar con ASP.NET Core MVC (controladores y respuestas).

namespace BackendUsuarios.Controllers;  // Define el espacio de nombres para este controlador (que maneja la lógica de los usuarios).

[ApiController]  // Indica que esta clase es un controlador de API y que maneja solicitudes HTTP.
[Route("api/[controller]")]  // Define la ruta base para las acciones del controlador. [controller] se reemplaza por el nombre del controlador, en este caso, 'users'.
public class UsersController : ControllerBase  // La clase hereda de ControllerBase, lo que la convierte en un controlador de API.
{
    private readonly AppDbContext _context;  // Declara una variable privada que almacenará el contexto de la base de datos (para interactuar con los datos).

    // Constructor que recibe el contexto de la base de datos como parámetro e inicializa la variable _context.
    public UsersController(AppDbContext context)
    {
        _context = context;  // Asigna el contexto recibido a la variable privada _context.
    }

    // Acción GET para obtener todos los usuarios de la base de datos
    [HttpGet]  // Define que esta acción responderá a solicitudes GET.
    public IActionResult GetUsers()
    {
        var users = _context.Users.ToList();  // Obtiene todos los usuarios de la base de datos y los convierte en una lista.
        return Ok(users);  // Devuelve una respuesta HTTP 200 con la lista de usuarios.
    }

    // Acción GET para obtener un usuario por su ID
    [HttpGet("{id:int}")]  // Define la ruta que recibirá un parámetro "id" de tipo entero en la URL.
    public IActionResult GetUserById(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);  // Busca el primer usuario que tenga el ID proporcionado.
        if (user == null)  // Si no se encuentra el usuario, devuelve una respuesta HTTP 404 (Not Found).
        {
            return NotFound(new { Message = $"User with ID {id} not found." });  // Devuelve un mensaje personalizado si el usuario no se encuentra.
        }
        return Ok(user);  // Si el usuario se encuentra, devuelve la información del usuario con una respuesta HTTP 200.
    }

    // Acción POST para crear un nuevo usuario
    [HttpPost]  // Define que esta acción responderá a solicitudes POST.
    public IActionResult CreateUser([FromBody] User user)  // Recibe los datos del nuevo usuario en el cuerpo de la solicitud (en formato JSON).
    {
        if (!ModelState.IsValid)  // Verifica si el modelo recibido es válido (basado en las validaciones del modelo User).
        {
            return BadRequest(ModelState);  // Si el modelo no es válido, devuelve un error 400 con los detalles de la validación.
        }

        // Verificar si el correo electrónico ya existe en la base de datos
        var emailExists = _context.Users.Any(u => u.Email == user.Email);  // Busca si ya existe un usuario con el mismo correo electrónico.
        if (emailExists)  // Si el correo ya está en uso, devuelve un error 400.
        {
            return BadRequest(new { Message = "The email is already in use." });  // Devuelve un mensaje de error indicando que el correo ya está en uso.
        }

        _context.Users.Add(user);  // Agrega el nuevo usuario a la base de datos.
        _context.SaveChanges();  // Guarda los cambios en la base de datos (inserta el nuevo usuario).

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);  // Devuelve una respuesta 201 (Created) con la URL para obtener el usuario recién creado.
    }

    // Acción PUT para actualizar un usuario existente
    [HttpPut("{id:int}")]  // Define la ruta que recibirá un parámetro "id" de tipo entero en la URL para identificar el usuario a actualizar.
    public IActionResult UpdateUser(int id, [FromBody] User updatedUser)  // Recibe el ID y los datos del usuario actualizado.
    {
        if (!ModelState.IsValid)  // Verifica si el modelo recibido es válido (basado en las validaciones del modelo User).
        {
            return BadRequest(ModelState);  // Si el modelo no es válido, devuelve un error 400 con los detalles de la validación.
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == id);  // Busca el usuario en la base de datos con el ID proporcionado.
        if (user == null)  // Si no se encuentra el usuario, devuelve una respuesta HTTP 404 (Not Found).
        {
            return NotFound(new { Message = $"User with ID {id} not found." });  // Devuelve un mensaje personalizado si el usuario no se encuentra.
        }

        // Verificar si el correo electrónico ya existe en otro usuario (no en el mismo usuario)
        var emailExists = _context.Users
                                  .Where(u => u.Id != id)  // Excluye al usuario actual para no verificar el correo del propio usuario.
                                  .Any(u => u.Email == updatedUser.Email);  // Verifica si ya existe otro usuario con el mismo correo electrónico.
        if (emailExists)  // Si el correo ya está en uso, devuelve un error 400.
        {
            return BadRequest(new { Message = "The email is already in use." });  // Devuelve un mensaje de error indicando que el correo ya está en uso.
        }

        // Actualiza los datos del usuario en la base de datos
        user.Name = updatedUser.Name;  // Actualiza el nombre del usuario.
        user.Email = updatedUser.Email;  // Actualiza el correo electrónico del usuario.
        user.Password = updatedUser.Password;  // Actualiza la contraseña del usuario.

        _context.SaveChanges();  // Guarda los cambios en la base de datos (actualiza los datos del usuario).

        return Ok(new { Message = $"User with ID {id} updated successfully.", User = user });  // Devuelve una respuesta 200 con el usuario actualizado.
    }

    // Acción DELETE para eliminar un usuario por su ID
    [HttpDelete("{id:int}")]  // Define la ruta que recibirá un parámetro "id" de tipo entero en la URL para identificar el usuario a eliminar.
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);  // Busca el usuario en la base de datos con el ID proporcionado.
        if (user == null)  // Si no se encuentra el usuario, devuelve una respuesta HTTP 404 (Not Found).
        {
            return NotFound(new { Message = $"User with ID {id} not found." });  // Devuelve un mensaje personalizado si el usuario no se encuentra.
        }

        _context.Users.Remove(user);  // Elimina el usuario de la base de datos.
        _context.SaveChanges();  // Guarda los cambios en la base de datos (elimina el usuario).

        return Ok(new { Message = $"User with ID {id} deleted successfully." });  // Devuelve una respuesta 200 indicando que el usuario fue eliminado correctamente.
    }
}
