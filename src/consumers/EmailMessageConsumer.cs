using MassTransit;
using shared.messages;
using censudex_clients_service.src.services;

namespace censudex_clients_service.src.consumers
{
    /// <summary>
    /// Consumidor de mensajes de correo electrónico utilizando el servicio SendGrid.
    /// </summary>
    public class EmailMessageConsumer : IConsumer<EmailMessage>
    {
        /// <summary>
        /// Consume el mensaje de correo electrónico.
        /// </summary>
        /// <param name="context">Contexto del mensaje de correo electrónico.</param>
        public async Task Consume(ConsumeContext<EmailMessage> context)
        {
            // Lógica para procesar el mensaje de correo electrónico recibido
            var emailMessage = context.Message;
            // Envío del correo electrónico utilizando el servicio SendGridService
            var emailSentResponse = await SendGridService.SendEmailAsync(emailMessage);
            // Retorno de la respuesta o excepción en caso de error
            if (emailSentResponse.StatusCode == System.Net.HttpStatusCode.Accepted || emailSentResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            else
            {
                throw new Exception("Error al enviar el correo electrónico");
            }
        }
    }
}