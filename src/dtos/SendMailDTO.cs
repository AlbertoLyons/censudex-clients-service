using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_clients_service.src.dtos
{
    /// <summary>
    /// DTO para el envío de correos electrónicos.
    /// </summary>
    public class SendMailDTO
    {
        // Correo electrónico del remitente.
        public required string FromEmail { get; set; }
        // Correo electrónico del destinatario.
        public required string ToEmail { get; set; }
        // Asunto del correo electrónico.
        public required string Subject { get; set; }
        // Contenido del correo electrónico en texto plano
        public required string PlainTextContent { get; set; }
        // Contenido del correo electrónico en HTML.
        public required string HtmlContent { get; set; }
    }
}