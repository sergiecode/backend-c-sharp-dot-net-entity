using System.Net;  // Importa las clases necesarias para trabajar con los códigos de estado HTTP, como InternalServerError.
using Newtonsoft.Json;  // Importa la librería Newtonsoft.Json para convertir objetos a formato JSON.

namespace BackendUsuarios.Middleware;  // Define el espacio de nombres para el middleware de manejo de excepciones.

public class ExceptionMiddleware  // Declara la clase ExceptionMiddleware que manejará las excepciones en las solicitudes.
{
    private readonly RequestDelegate _next;  // Define una variable privada que guarda el siguiente componente del pipeline de la solicitud HTTP.

    public ExceptionMiddleware(RequestDelegate next)  // Constructor que recibe el siguiente componente del pipeline como parámetro.
    {
        _next = next;  // Asigna el siguiente componente del pipeline a la variable _next.
    }

    public async Task InvokeAsync(HttpContext context)  // Método que maneja la solicitud HTTP.
    {
        try
        {
            await _next(context);  // Llama al siguiente componente del pipeline, que procesará la solicitud.
        }
        catch (Exception ex)  // Si ocurre una excepción durante la ejecución del siguiente componente, la captura.
        {
            await HandleExceptionAsync(context, ex);  // Llama al método para manejar la excepción y devolver una respuesta adecuada.
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)  // Método que maneja la excepción y devuelve una respuesta con el error.
    {
        context.Response.ContentType = "application/json";  // Establece el tipo de contenido de la respuesta a JSON.
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;  // Establece el código de estado de la respuesta a "500 Internal Server Error".

        // Crea un objeto anónimo con el estado de la respuesta, un mensaje genérico de error y los detalles de la excepción (se pueden ocultar en producción).
        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred. Please try again later.",
            Details = exception.Message // Detalles de la excepción que pueden ser útiles para depuración, pero que no deberían mostrarse en producción.
        };

        // Serializa el objeto de respuesta a formato JSON y lo escribe en el cuerpo de la respuesta.
        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
