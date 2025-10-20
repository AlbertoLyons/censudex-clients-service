using SendGrid;
using SendGrid.Helpers.Mail;

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
        /// <param name="fromMail"></param>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="plainTextContent"></param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public static async Task<Response> SendEmailAsync(string fromMail, string toEmail, string subject, string plainTextContent, string htmlContent)
        {
            // Obtener la clave API de SendGrid desde las variables de entorno.
            var apiKey = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY");
            // Crear el cliente de SendGrid y el mensaje de correo electrónico.
            var client = new SendGridClient(apiKey);
            // Configurar el correo electrónico.
            var from = new EmailAddress(fromMail, "Censudex Clients Service");
            // Configurar el destinatario del correo.
            var to = new EmailAddress(toEmail);
            // Crear el mensaje de correo electrónico.
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );
            // Enviar el correo electrónico y devolver la respuesta.
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}