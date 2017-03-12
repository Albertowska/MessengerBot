using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessengerBot.Models
{
    public class ComplexMessage
    {
        public Recipient recipient { get; set; }
        public ExtraMessage message { get; set; }
    }

    public class ExtraMessage
    {
        public AttachmentMessage attachment { get; set; }
    }

    public class AttachmentMessage
    {
        public string type { get; set; }
        public PayloadMessage payload { get; set; }
    }

    public class PayloadMessage
    {
        public string template_type { get; set; }
        public List<ElementMessage> elements { get; set; }
    }

    public class ElementMessage
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public string item_url { get; set; }
        public string image_url { get; set; }
        public List<ButtonMessage> buttons { get; set; }
    }

    public class ButtonMessage
    {
        public string type { get; set; }
        public string url { get; set; }
        public string title { get; set; }
    }
}