using SendGrid;
using SendGrid.Helpers.Mail;
using shared.messages;

namespace censudex_clients_service.src.services
{
    /// <summary>
    /// Servicio para enviar correos electrónicos usando SendGrid.
    /// </summary>
    public class SendGridService
    {
        /// <summary>
        /// Función para envíar los correos electrónicos.
        /// </summary>
        /// <param name="emailMessage"></param>
        /// <returns></returns>
        public static async Task<Response> SendEmailAsync(EmailMessage emailMessage)
        {
            // Obtener la clave API de SendGrid desde las variables de entorno.
            var apiKey = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY");
            // Crear el cliente de SendGrid y el mensaje de correo electrónico.
            var client = new SendGridClient(apiKey);
            // Configurar el correo electrónico.
            var from = new EmailAddress(emailMessage.From, "Censudex Clients Service");
            // Configurar el destinatario del correo.
            var to = new EmailAddress(emailMessage.To);
            // Crear el mensaje de correo electrónico.
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                emailMessage.Subject,
                emailMessage.PlainTextContent,
                emailMessage.HtmlContent
            );
            // Enviar el correo electrónico y devolver la respuesta.
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}