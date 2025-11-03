using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace shared.messages
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string PlainTextContent { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        private DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}